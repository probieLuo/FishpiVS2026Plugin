using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishpiVS2026Plugin.Helpers;
using FishpiVS2026Plugin.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FishpiVS2026Plugin.ViewModels
{
    public partial class ChatRoomViewModel: ObservableObject
    {
        private ChatRoomClient roomClient;
        private readonly string domain = "rhyus-chengdu.fishpi.cn:10832";
        private readonly string apikey = "a52b920d8994a75a6791735191d68c53c027cf1d3897aa3ee19230faf464711523fc876a05830a4baef01c78fca2173995225ceee03738296be97e3eb61b3cf409c9fcb1cccdf415911e9f0c6956f335513afd8f6808b5c85fc8c0dab5d1fcdb";

        private ObservableCollection<ChatRoomMessage> _messages = new ObservableCollection<ChatRoomMessage>();
        public ObservableCollection<ChatRoomMessage> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value); // 触发属性通知
        }

        public IAsyncRelayCommand OnLoadedCommand { get; }
        public ChatRoomViewModel()
        {
            OnLoadedCommand = new AsyncRelayCommand(OnLoadedAsync);   

            roomClient = new ChatRoomClient(domain, apikey);
            roomClient.OnMessageReceived += (message) =>
            {
                // 在UI线程上更新Messages集合
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(message);
                });
            };
        }

        private async Task OnLoadedAsync()
        {
            await roomClient.StartAsync();
        }
    }
}
