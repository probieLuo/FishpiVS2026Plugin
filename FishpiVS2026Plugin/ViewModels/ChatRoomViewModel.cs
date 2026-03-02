using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishpiVS2026Plugin.Helpers;
using FishpiVS2026Plugin.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace FishpiVS2026Plugin.ViewModels
{
    public partial class ChatRoomViewModel: ObservableObject
    {
        private ChatRoomClient roomClient;
        private HttpRestClient httpRestClient;
        private int messagesMax = 10000;
        private const string CollectionName = "FishpiVS2026Plugin";
        private ObservableCollection<ChatRoomMessage> _messages = new ObservableCollection<ChatRoomMessage>();
        public ObservableCollection<ChatRoomMessage> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value); 
        }

        private string _sendContent = "";

		public string SendContent
		{
			get => _sendContent;
			set => SetProperty(ref _sendContent, value); 
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

        public IAsyncRelayCommand OnLoadedCommand { get; }

        public IAsyncRelayCommand OnSendCommand { get; }

        public RelayCommand OnSettingsCommand { get; }

        public IAsyncRelayCommand OnSaveSettingsCommand { get; }

        public ChatRoomViewModel()
        {
            OnLoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
            OnSendCommand = new AsyncRelayCommand(OnSendAsync);
            OnSettingsCommand = new RelayCommand(OnSettings);
            OnSaveSettingsCommand = new AsyncRelayCommand(OnSaveSettingsAsync);
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
            ChatRoomSendMessage msg = new ChatRoomSendMessage
            {
                ApiKey = Apikey,
                Client = "Other",
                Content = SendContent
            };
            var response = await httpRestClient.PostAsync<ChatRoomSendMessage>("chat-room/send", msg);
            SendContent = "";
		}

		private async Task OnLoadedAsync()
        {
            LoadSettings();

            httpRestClient = new HttpRestClient("https://fishpi.cn/");
            roomClient = new ChatRoomClient(Domain, Apikey);
            roomClient.OnMessageReceived += RoomClient_OnMessageReceived; ;

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
