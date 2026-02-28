using FishpiVS2026Plugin.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FishpiVS2026Plugin.Helpers
{
    public class ChatRoomClient
    {
        private ClientWebSocket _webSocket;
        private readonly string _wssUrl;
        private CancellationTokenSource _cts;
        private Timer _heartbeatTimer; // 心跳定时器（3分钟一次）
        private readonly int _reconnectDelayMs = 5000; // 重连延迟（5秒）

        public event Action<ChatRoomMessage> OnMessageReceived; // 外部订阅事件：收到JSON消息

        // 构造函数：传入WSS地址和apiKey
        public ChatRoomClient(string domain, string apiKey)
        {
            _wssUrl = $"wss://{domain}/chat-room-channel?apiKey={apiKey}";
        }

        public async Task StartAsync()
        {
            while (true)
            {
                try
                {
                    await ConnectAndHandleEventsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"客户端异常：{ex.Message}");
                }

                // 连接关闭/出错后，等待并重连
                Console.WriteLine($"将在 {_reconnectDelayMs / 1000} 秒后尝试重连...");
                await Task.Delay(_reconnectDelayMs);
            }
        }

        private async Task ConnectAndHandleEventsAsync()
        {
            // 初始化资源
            _cts = new CancellationTokenSource();
            _webSocket = new ClientWebSocket();

            try
            {
                Console.WriteLine($"正在连接 WSS 地址：{_wssUrl}");
                await _webSocket.ConnectAsync(new Uri(_wssUrl), _cts.Token);
                Console.WriteLine("=== Open：WSS连接成功 ===");

                // 启动3分钟心跳定时器
                StartHeartbeatTimer();

                // 持续监听消息和连接状态
                var buffer = new byte[4096];
                while (_webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    // 服务端主动关闭
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("=== Close：服务端发起关闭连接 ===");
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "正常关闭", _cts.Token);
                        break;
                    }

                    // Message：收到JSON消息
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"=== Message：收到原始JSON → {jsonMessage} ===");
                    HandleReceivedJsonMessage(jsonMessage);

                    try
                    {
                        // 反序列化
                        ChatRoomMessage message = JsonSerializer.Deserialize<ChatRoomMessage>(jsonMessage);
                        Console.WriteLine($"JSON解析结果 → 类型：{message.Type}，内容：{message.Content}，时间：{message.Time}");

                        // 业务
                        OnMessageReceived?.Invoke(message);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON反序列化失败：{ex.Message}，原始内容：{jsonMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Error：连接/通信出错
                Console.WriteLine($"=== Error：{ex.Message} ===");
            }
            finally
            {
                // 清理资源
                StopHeartbeatTimer(); // 停止心跳
                await CloseConnectionAsync(); // 关闭连接
                Console.WriteLine("=== Close：客户端清理连接资源 ===");
            }
        }

        private void StartHeartbeatTimer()
        {
            _heartbeatTimer = new Timer(
                async (state) => await SendHeartbeatAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(3));
            Console.WriteLine("心跳定时器已启动（每3分钟发送-hb-）");
        }

        private void StopHeartbeatTimer()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            Console.WriteLine("心跳定时器已停止");
        }

        private async Task SendHeartbeatAsync()
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("心跳发送失败：连接未打开");
                return;
            }

            try
            {
                byte[] heartbeatBytes = Encoding.UTF8.GetBytes("-hb-");
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(heartbeatBytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    _cts.Token);
                Console.WriteLine($"发送心跳包：-hb-（{DateTime.Now:HH:mm:ss}）");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"心跳发送异常：{ex.Message}");
            }
        }

        private void HandleReceivedJsonMessage(string jsonString)
        {
            try
            {
                // 反序列化
                ChatRoomMessage message = JsonSerializer.Deserialize<ChatRoomMessage>(jsonString);
                Console.WriteLine($"JSON解析结果 → 类型：{message.Type}，内容：{message.Content}，时间：{message.Time}");

                // 业务
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON反序列化失败：{ex.Message}，原始内容：{jsonString}");
            }
        }

        private async Task CloseConnectionAsync()
        {
            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端关闭", CancellationToken.None);
                }
                _webSocket.Dispose();
            }
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
