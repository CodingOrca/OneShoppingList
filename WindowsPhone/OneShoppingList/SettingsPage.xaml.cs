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

namespace OneShoppingList
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        SettingsViewModel viewModel = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void signInButton_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == Microsoft.Live.LiveConnectSessionStatus.Connected)
            {
                LiveConnectClient client = new LiveConnectClient(e.Session);
                client.GetCompleted += new System.EventHandler<LiveOperationCompletedEventArgs>(client_GetCompleted); ;
                client.GetAsync("me");
            }
            else
            {
                viewModel.LoggedOut();
            }
        }

        void  client_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                viewModel.UserName = e.Result["name"] as string;
            }
            else
            {
                MessageBox.Show("Error calling API: " + e.Error.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Reset();
        }

    }
}