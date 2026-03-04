using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishpiVS2026Plugin.Helpers;
using FishpiVS2026Plugin.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using RestSharp;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace FishpiVS2026Plugin.ViewModels
{
    public partial class ChatRoomViewModel: ObservableObject
    {
        private ChatRoomClient roomClient;
        private HttpRestClient httpRestClient;
        private int messagesMax = 10000;
        private const string CollectionName = "FishpiVS2026Plugin";
        private readonly string baseurl = "https://fishpi.cn";

		#region DesignData
		private ObservableCollection<ChatRoomMessage> _messages = new ObservableCollection<ChatRoomMessage>();
        public ObservableCollection<ChatRoomMessage> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value); 
        }

        private ChatRoomMessage _selectedMessage;
        public ChatRoomMessage SelectedMessage
        {
            get => _selectedMessage;
            set => SetProperty(ref _selectedMessage, value);
        }

        private ObservableCollection<BreezemoonItem> _breezemoons = new ObservableCollection<BreezemoonItem>();
        public ObservableCollection<BreezemoonItem> Breezemoons
        {
            get => _breezemoons;
            set => SetProperty(ref _breezemoons, value);
        }

        private string _refUserName = "";

        public string RefUserName
        {
            get => _refUserName;
            set => SetProperty(ref _refUserName, value);
        }

        private string _refContent = "";

        public string RefContent
        {
            get => _refContent;
            set => SetProperty(ref _refContent, value);
        }

        private string _sendContent = "";

		public string SendContent
		{
			get => _sendContent;
			set => SetProperty(ref _sendContent, value); 
		}

        private string _publishBreezemoon = "";

        public string PublishBreezemoon
        {
            get => _publishBreezemoon;
            set => SetProperty(ref _publishBreezemoon, value);
        }
        
        private string _apikey = "";

        public string Apikey
        {
            get => _apikey;
            set => SetProperty(ref _apikey, value);
        }

        private string _domain = "";

        public string Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }

        private Visibility _chatViewVisibility = Visibility.Visible;

        public Visibility ChatViewVisibility
        {
            get => _chatViewVisibility;
            set => SetProperty(ref _chatViewVisibility, value);
        }

        private Visibility _settingsViewVisibility = Visibility.Hidden;

        public Visibility SettingsViewVisibility
        {
            get => _settingsViewVisibility;
            set => SetProperty(ref _settingsViewVisibility, value);
        }

        private Visibility _refViewVisibility = Visibility.Collapsed;

        public Visibility RefViewVisibility
        {
            get => _refViewVisibility;
            set => SetProperty(ref _refViewVisibility, value);
        }
		#endregion

		#region Commands
		public IAsyncRelayCommand OnLoadedCommand { get; }

        public IAsyncRelayCommand OnSendCommand { get; }

        public RelayCommand OnSettingsCommand { get; }

        public IAsyncRelayCommand OnSaveSettingsCommand { get; }

        public RelayCommand OnCancelRefCommand { get; }

        public RelayCommand OnOpenRefCommand { get; }

        public RelayCommand OnCopyMsgCommand { get; }

        public IAsyncRelayCommand OnRefreshBreezemoonsCommand { get; }

        public IAsyncRelayCommand OnPublishBreezemoonCommand { get; }
		#endregion

		public ChatRoomViewModel()
        {
            OnLoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
			OnSendCommand = new AsyncRelayCommand(OnSendAsync);
            OnSettingsCommand = new RelayCommand(OnSettings);
            OnSaveSettingsCommand = new AsyncRelayCommand(OnSaveSettingsAsync);
            OnCancelRefCommand = new RelayCommand(() => RefViewVisibility = Visibility.Collapsed);
            OnOpenRefCommand = new RelayCommand(OnOpenRef);
            OnCopyMsgCommand = new RelayCommand(() =>
            {
                if (SelectedMessage != null)
                {
                    System.Windows.Clipboard.SetText(SelectedMessage.Md);
                }
            });
            OnRefreshBreezemoonsCommand = new AsyncRelayCommand(OnRefreshBreezemoonsAsync);
            OnPublishBreezemoonCommand = new AsyncRelayCommand(OnPublishBreezemoonAsync);
        }

        private async Task OnPublishBreezemoonAsync()
        {
            BreezemoonRequest msg = new BreezemoonRequest
            {
                ApiKey = Apikey,
                BreezemoonContent = PublishBreezemoon
            };
            var response = await httpRestClient.PostAsync<BreezemoonRootResponse>("breezemoon", msg);
            await OnRefreshBreezemoonsAsync();
            PublishBreezemoon = "";
        }

        private async Task OnRefreshBreezemoonsAsync()
        {
            Breezemoons.Clear();
            Parameter[] parameters =
            {
                Parameter.CreateParameter("p",1, ParameterType.QueryString),
                Parameter.CreateParameter("size",50, ParameterType.QueryString),
            };

            var response = await httpRestClient.GetAsync<BreezemoonRootResponse>("api/breezemoons", parameters);
            if (response != null && response.IsSuccessful && response.Data != null)
            {
                foreach (var item in response.Data.Breezemoons)
                {
                    Breezemoons.Add(item);
                }
            }
        }

        private void OnOpenRef()
        {
            RefViewVisibility = Visibility.Visible;
            RefUserName = SelectedMessage?.UserName ?? "";
            RefContent = SelectedMessage?.Md ?? "";
        }

        private void LoadSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 获取VS的设置存储
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            // 创建配置集合（如果不存在）
            if (!userSettingsStore.CollectionExists(CollectionName))
            {
                userSettingsStore.CreateCollection(CollectionName);
            }
            Domain = userSettingsStore.GetString(CollectionName, "Domain","");
            Apikey = userSettingsStore.GetString(CollectionName, "Apikey","");
        }

        private async Task OnSaveSettingsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            userSettingsStore.SetString(CollectionName, "Domain", Domain);
            userSettingsStore.SetString(CollectionName, "Apikey", Apikey);

            // 更新客户端配置
            if(roomClient!= null)
            {
                await roomClient.StopAsync();
                roomClient.OnMessageReceived -= RoomClient_OnMessageReceived;
            }
            roomClient = new ChatRoomClient(Domain, Apikey);
            roomClient.OnMessageReceived += RoomClient_OnMessageReceived;
            _ = roomClient.StartAsync();

            ChatViewVisibility = Visibility.Visible;
            SettingsViewVisibility = Visibility.Hidden;
        }

        private void OnSettings()
        {
            if (ChatViewVisibility == Visibility.Visible)
            {
                ChatViewVisibility = Visibility.Hidden;
                SettingsViewVisibility = Visibility.Visible;
            }
            else
            {
                ChatViewVisibility = Visibility.Visible;
                SettingsViewVisibility = Visibility.Hidden;
            }
        }

        private async Task OnSendAsync()
		{
            if (RefViewVisibility!=Visibility.Visible)
            {
                ChatRoomSendMessage msg = new ChatRoomSendMessage
                {
                    ApiKey = Apikey,
                    Client = "Other",
                    Content = SendContent
                };
                var response = await httpRestClient.PostAsync<ChatRoomSendMessage>("chat-room/send", msg);
            }
            else
            {
                SendContent = SendContent + $"\n\n##### 引用 @{SelectedMessage.UserName} [↩]({baseurl}/cr#chatroom{SelectedMessage.OId} \"跳转至原消息\")  \n> {SelectedMessage.Md}\n";
                ChatRoomSendMessage msg = new ChatRoomSendMessage
                {
                    ApiKey = Apikey,
                    Client = "Other",
                    Content = SendContent
                };
                var response = await httpRestClient.PostAsync<ChatRoomSendMessage>("chat-room/send", msg);
                RefViewVisibility = Visibility.Collapsed;
            }
            SendContent = "";
		}

		private async Task OnLoadedAsync()
        {
            LoadSettings();

            httpRestClient = new HttpRestClient(baseurl);
            roomClient = new ChatRoomClient(Domain, Apikey);
            roomClient.OnMessageReceived += RoomClient_OnMessageReceived; ;

            //获取清风明月
            await OnRefreshBreezemoonsAsync();

            await roomClient.StartAsync();
        }

        private void RoomClient_OnMessageReceived(ChatRoomMessage message)
        {
            if (Messages.Count > messagesMax)
            {
                Messages.RemoveAt(0);

            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }
    }
}
