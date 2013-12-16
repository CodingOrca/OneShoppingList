using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace OneShoppingList.View
{
    public partial class MorePage : PhoneApplicationPage
    {
        public MorePage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);
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

        private void Support_Button_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new Uri("http://oneshoppinglist.wordpress.com");
            browser.Show();
        }

        private void About_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        private void Apps_Button_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();
            marketplaceSearchTask.SearchTerms = "artur pusztai";
            marketplaceSearchTask.Show();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("MorePage");
        }
    }
}