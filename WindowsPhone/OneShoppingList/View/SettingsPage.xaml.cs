using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using OneShoppingList;
using Microsoft.Live;
using OneShoppingList.ViewModel;
using YourLastAboutDialog;

namespace OneShoppingList
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        SettingsViewModel viewModel;

        public SettingsPage()
        {
            InitializeComponent();
            viewModel = (App.Current.Resources["Locator"] as ViewModelLocator).Settings;
            Framework.WPHacks.WireOrientationHack(this);
            this.DataContext = viewModel;
        }

        private void signInButton_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            isLoggingOut = false;
            if (e.Status == Microsoft.Live.LiveConnectSessionStatus.Connected)
            {
                LiveConnectClient client = new LiveConnectClient(e.Session);
                client.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(client_GetCompleted); ;
                client.GetAsync("me");
            }
            else
            {
                isLoggingOut = true;
                viewModel.LoggedOut();
            }
        }

        private bool isLoggingOut = false;

        void  client_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Error == null )
            {
                if (!isLoggingOut)
                {
                    viewModel.UserName = e.Result["name"] as string;
                }
            }
            else
            {
                MessageBox.Show("Error calling API: " + e.Error.ToString());
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

    }
}