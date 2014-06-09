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
using OneShoppingList.Model;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using OneShoppingList.Resources;
using System.Text;

namespace OneShoppingList
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        SettingsViewModel viewModel;
        ViewModelLocator locator;


        public SettingsPage()
        {
            InitializeComponent();
            locator = App.Current.Resources["Locator"] as ViewModelLocator;
            viewModel = (App.Current.Resources["Locator"] as ViewModelLocator).Settings;
            Framework.WPHacks.WireOrientationHack(this);
            locator.Main.SyncCommand.CanExecuteChanged += SyncCommand_CanExecuteChanged;
        }

        void SyncCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            signInButton.IsEnabled = DeviceNetworkInformation.IsNetworkAvailable;
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
                if (e.Error != null)
                {
                    if (DeviceNetworkInformation.IsNetworkAvailable)
                    {
                        string error = "";
                        if (!String.IsNullOrEmpty(e.Error.Message))
                        {
                            error = e.Error.Message;
                        }
                        else if (e.Error.InnerException != null)
                        {
                            error = e.Error.InnerException.Message;
                        }
                        if (String.IsNullOrEmpty(error))
                        {
                            error = "Connection Error";
                        }
                        MessageBox.Show(error, "ERROR", MessageBoxButton.OK);
                    }
                    // else, silently ignore 
                }
                else
                {
                    isLoggingOut = true;
                    viewModel.LoggedOut();
                }
            }
            this.Dispatcher.BeginInvoke(() =>
                {
                    signInButton.IsEnabled = DeviceNetworkInformation.IsNetworkAvailable;
                }
            );
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
            signInButton.IsEnabled = DeviceNetworkInformation.IsNetworkAvailable;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ToolbarEvents", "ToolbarButton", "ToolbarButtonSync", 0);
            locator.Main.SyncCommand.Execute(null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DataLocator.Current.ProductItems.Clear();
            DataLocator.Current.Shops.Clear();
            locator.Main.ResetSyncHandler();
        }

        private bool RefreshIsTrial = false;
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("SettingsPage");
            if (this.RefreshIsTrial)
            {
                viewModel.RefreshIsTrial();
                this.RefreshIsTrial = false;
            }
            signInButton.IsEnabled = DeviceNetworkInformation.IsNetworkAvailable;
            viewModel.RefreshAllNotofications();
        }

        private void Paths_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/ShopConfigurationPage.xaml", UriKind.Relative));
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }

        private void Rate_Button_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void Buy_Button_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.Show();
            this.RefreshIsTrial = true;
        }

        private void Support_Button_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://oneshoppinglist.wordpress.com/support");
            browser.Show();
        }

        private void About_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/YourLastAboutDialog;component/AboutPage.xaml?{0}=1", AboutPage.SelectedPivotItemIndexKey), UriKind.Relative));
        }

        private void Apps_Button_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();
            marketplaceSearchTask.SearchTerms = "artur pusztai";
            marketplaceSearchTask.Show();
        }

        private void NewThisVersion_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/YourLastAboutDialog;component/AboutPage.xaml?{0}=2", AboutPage.SelectedPivotItemIndexKey), UriKind.Relative));
        }

        private void Vote_Button_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://oneshoppinglist.wordpress.com/backlog");
            browser.Show();
        }

        private void Bug_Button_Click(object sender, EventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.Subject = "One Shopping List";
            task.To = "artur.pusztai@live.com";
            task.Show();
        }

        private void Share_Button_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AppResources.IrecommendYou);
            sb.AppendLine(AppResources.appName);
            sb.AppendLine("Homepage: http://OneShoppingList.wordpress.com");
            sb.AppendLine("WP7/8: http://www.windowsphone.com/de-de/store/app/one-shopping-list/d2dbb41e-dd09-4621-b9f1-f6f5d5f7ab1b");
            sb.AppendLine("Windows 7: http://oneshoppinglist.codeplex.com/");

            EmailComposeTask task = new EmailComposeTask();
            task.Body = sb.ToString();
            task.Subject = AppResources.appName;
            task.Show();
        }

    }
}