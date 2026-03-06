using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishpiVS2026Plugin.Helpers;
using FishpiVS2026Plugin.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private int _currentChatRoomPage = 1;
        private int CurrentBreezemoonsPage
        {
            get { return _currentChatRoomPage; }
            set
            {
                if(value < 1) return;
                _currentChatRoomPage = value;
            }
        }
        private int sizePerPage = 100;

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

        private BreezemoonItem _selectedBreezemoon;
        public BreezemoonItem SelectedBreezemoon
        {
            get => _selectedBreezemoon;
            set => SetProperty(ref _selectedBreezemoon, value);
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

        private string _messageTail = "";

        public string MessageTail
        {
            get => _messageTail;
            set => SetProperty(ref _messageTail, value);
        }

        private string _shieldUsers = "";

        public string ShieldUsers
        {
            get => _shieldUsers;
            set => SetProperty(ref _shieldUsers, value);
        }

        private Visibility _refViewVisibility = Visibility.Collapsed;

        public Visibility RefViewVisibility
        {
            get => _refViewVisibility;
            set => SetProperty(ref _refViewVisibility, value);
        }

        private double _activity = 0;

        public double Activity
        {
            get => _activity;
            set => SetProperty(ref _activity, value);
        }
        #endregion

        #region Commands
        public IAsyncRelayCommand OnLoadedCommand { get; }

        public IAsyncRelayCommand OnSendCommand { get; }

        public IAsyncRelayCommand OnSaveSettingsCommand { get; }

        public RelayCommand OnCancelRefCommand { get; }

        public RelayCommand OnOpenRefCommand { get; }

        public RelayCommand OnCopyMsgCommand { get; }

        public IAsyncRelayCommand OnRefreshBreezemoonsCommand { get; }

        public IAsyncRelayCommand OnPublishBreezemoonCommand { get; }

        public IAsyncRelayCommand OnPreviousPageBreezemoonsCommand { get; }

        public IAsyncRelayCommand OnNextPageBreezemoonsCommand { get; }

        public RelayCommand OnCopyBreezemoonCommand { get; }

        public IAsyncRelayCommand OnSaveOtherSettingsCommand {  get; }

        public IAsyncRelayCommand OnGetActivityCommand { get; }

        public IAsyncRelayCommand OnGetActivityRewardCommand { get; }
        #endregion

        public ChatRoomViewModel()
        {
            OnLoadedCommand = new AsyncRelayCommand(OnLoadedAsync);
			OnSendCommand = new AsyncRelayCommand(OnSendAsync);
            OnSaveSettingsCommand = new AsyncRelayCommand(OnSaveSettingsAsync);
            OnSaveOtherSettingsCommand = new AsyncRelayCommand(OnSaveOtherSettingsAsync);
            OnCancelRefCommand = new RelayCommand(() => RefViewVisibility = Visibility.Collapsed);
            OnOpenRefCommand = new RelayCommand(OnOpenRef);
            OnCopyMsgCommand = new RelayCommand(() =>
            {
                if (SelectedMessage != null)
                {
                    System.Windows.Clipboard.SetText(SelectedMessage.Md);
                }
            });
            OnCopyBreezemoonCommand = new RelayCommand(() =>
            {
                if (SelectedBreezemoon != null)
                {
                    string innerText = "";
                    int startIndex = SelectedBreezemoon.BreezemoonContent.IndexOf("<p>", StringComparison.OrdinalIgnoreCase);
                    int endIndex = SelectedBreezemoon.BreezemoonContent.IndexOf("</p>", StringComparison.OrdinalIgnoreCase);

                    if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
                        innerText = SelectedBreezemoon.BreezemoonContent;

                    startIndex += 3;
                    innerText = SelectedBreezemoon.BreezemoonContent.Substring(startIndex, endIndex - startIndex);
                    System.Windows.Clipboard.SetText(innerText);
                }
            });
            OnRefreshBreezemoonsCommand = new AsyncRelayCommand(OnRefreshBreezemoonsAsync);
            OnPublishBreezemoonCommand = new AsyncRelayCommand(OnPublishBreezemoonAsync);
            OnPreviousPageBreezemoonsCommand = new AsyncRelayCommand(OnPreviousPageBreezemoonsAsync);
            OnNextPageBreezemoonsCommand = new AsyncRelayCommand(OnNextPageBreezemoonsAsync);
            OnGetActivityCommand = new AsyncRelayCommand(OnGetActivityAsync);
            OnGetActivityRewardCommand = new AsyncRelayCommand(OnGetActivityRewardAsync);
        }

        private async Task OnGetActivityAsync()
        {
            RestSharp.Parameter[] parameters =
            {
                RestSharp.Parameter.CreateParameter("apiKey", Apikey, ParameterType.QueryString),
            };
            var response = await httpRestClient.GetAsync<LivenessResponse>("/user/liveness", parameters);
            if(response != null && response.IsSuccessful && response.Data != null)
            {
                Activity = Math.Round(response.Data.Liveness,0);
            }
        }

        private async Task OnGetActivityRewardAsync()
        {
            RestSharp.Parameter[] parameters =
            {
                RestSharp.Parameter.CreateParameter("apiKey", Apikey, ParameterType.QueryString),
            };
            var response = await httpRestClient.GetAsync<LivenessrewardResponse>("/activity/yesterday-liveness-reward-api", parameters);
        }

        private async Task OnSaveOtherSettingsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            userSettingsStore.SetString(CollectionName, "MessageTail", MessageTail);
            userSettingsStore.SetString(CollectionName, "ShieldUsers", ShieldUsers);
        }

        private async Task OnNextPageBreezemoonsAsync()
        {
            var list = await GetBreezemoonsAsync(CurrentBreezemoonsPage + 1, sizePerPage);
            if (list != null && list.Count > 0)
            {
                CurrentBreezemoonsPage++;
                Breezemoons.Clear();
                foreach (var item in list)
                {
                    Breezemoons.Add(item);
                }
            }
        }

        private async Task OnPreviousPageBreezemoonsAsync()
        {
            var list = await GetBreezemoonsAsync(CurrentBreezemoonsPage - 1, sizePerPage);
            if (list != null && list.Count > 0)
            {
                CurrentBreezemoonsPage--;
                Breezemoons.Clear();
                foreach (var item in list)
                {
                    Breezemoons.Add(item);
                }
            }
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
                Parameter.CreateParameter("p", 1, ParameterType.QueryString),
                Parameter.CreateParameter("size", sizePerPage, ParameterType.QueryString),
            };

            var response = await httpRestClient.GetAsync<BreezemoonRootResponse>("api/breezemoons", parameters);
            if (response != null && response.IsSuccessful && response.Data != null)
            {
                foreach (var item in response.Data.Breezemoons)
                {
                    Breezemoons.Add(item);
                }
                CurrentBreezemoonsPage = 1;
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
            MessageTail = userSettingsStore.GetString(CollectionName, "MessageTail", "");
            ShieldUsers = userSettingsStore.GetString(CollectionName, "ShieldUsers", "");
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
        }

        private async Task OnSendAsync()
		{
            string Content = "";
            if(!string.IsNullOrWhiteSpace(MessageTail))
            {
                Content = SendContent + "\t\n> " + MessageTail;
            }
            if (RefViewVisibility!=Visibility.Visible)
            {
                ChatRoomSendMessage msg = new ChatRoomSendMessage
                {
                    ApiKey = Apikey,
                    Client = "Other",
                    Content = Content
                };
                var response = await httpRestClient.PostAsync<ChatRoomSendMessage>("chat-room/send", msg);
            }
            else
            {
                Content = SendContent + $"\n\n##### 引用 @{SelectedMessage.UserName} [↩]({baseurl}/cr#chatroom{SelectedMessage.OId} \"跳转至原消息\")  \n> {SelectedMessage.Md}\n";
                ChatRoomSendMessage msg = new ChatRoomSendMessage
                {
                    ApiKey = Apikey,
                    Client = "Other",
                    Content = Content
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

            await OnGetActivityAsync();

            await roomClient.StartAsync();
        }

        private void RoomClient_OnMessageReceived(ChatRoomMessage message)
        {
            if (Messages.Count > messagesMax)
            {
                Messages.RemoveAt(0);

            }

            #region 屏蔽用户
            if (!string.IsNullOrEmpty(ShieldUsers))
            {
                string[] shieldUsers = ShieldUsers.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (shieldUsers != null && shieldUsers.Length > 0)
                {
                    if (shieldUsers.Contains(message.UserName))
                    {
                        return;
                    }
                }
            }
            #endregion

            if (string.IsNullOrEmpty(message.Md))
                return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
            });
        }

        public async Task<List<BreezemoonItem>> GetBreezemoonsAsync(int page,int size = 50)
        {
            Parameter[] parameters =
            {
                Parameter.CreateParameter("p", page, ParameterType.QueryString),
                Parameter.CreateParameter("size", size, ParameterType.QueryString),
            };

            var response = await httpRestClient.GetAsync<BreezemoonRootResponse>("api/breezemoons", parameters);
            if (response != null && response.IsSuccessful && response.Data != null)
            {
                return response.Data.Breezemoons;
            }
            return null;
        }
    }
}
