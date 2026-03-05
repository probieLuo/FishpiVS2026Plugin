using FishpiVS2026Plugin.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FishpiVS2026Plugin.Views
{
    /// <summary>
    /// Interaction logic for ChatRoomControl.
    /// </summary>
    public partial class ChatRoomControl : UserControl
    {
		public ChatRoomViewModel ViewModel { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomControl"/> class.
        /// </summary>
        public ChatRoomControl()
        {
			try
			{
				//todo 或可考虑程序集遍历
				var asmFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var candidate = Path.Combine(asmFolder ?? string.Empty, "MaterialDesignThemes.Wpf.dll");
				var candidate2 = Path.Combine(asmFolder ?? string.Empty, "Microsoft.Xaml.Behaviors.dll");
				var candidate3 = Path.Combine(asmFolder ?? string.Empty, "RestSharp.dll");
				var candidate4 = Path.Combine(asmFolder ?? string.Empty, "System.Threading.Tasks.Extensions.dll");
				if (File.Exists(candidate))
				{
					Assembly.LoadFrom(candidate);
				}
				if (File.Exists(candidate2))
				{
					Assembly.LoadFrom(candidate2);
				}
				if (File.Exists(candidate4))
				{
					Assembly.LoadFrom(candidate4);
				}
				if (File.Exists(candidate3))
				{
					Assembly.LoadFrom(candidate3);
				}
			}
			catch
			{
				
			}

			this.InitializeComponent();
            ViewModel = new ChatRoomViewModel();
            this.DataContext = ViewModel;

			//viewModel.Messages.CollectionChanged += Messages_CollectionChanged;

		}

		private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					if (ChatListView.Items.Count > 0)
					{
						ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]); 
					}
				}));
			}
		}

		/// <summary>
		/// Handles click on the button by displaying a message box.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ChatRoom");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
			ReleaseDialogHost.IsOpen = false;
            ViewModel.OnPublishBreezemoonCommand.Execute(null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void ArrowBottomButton_Click(object sender, RoutedEventArgs e)
        {
			ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]);
        }
    }
}