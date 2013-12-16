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
using OneShoppingList.Model;
using Microsoft.Phone.Shell;
using OneShoppingList.ViewModel;
using OneShoppingList.Resources;

namespace OneShoppingList.View
{
    public partial class ShopConfigurationPage : PhoneApplicationPage
    {
        MainViewModel viewModel;

        public ShopConfigurationPage()
        {
            InitializeComponent();
            appbar_up = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            appbar_renameCat = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            appbar_down = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            appbar_newShop = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            appbar_renameShop = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            appbar_deleteShop = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            Framework.WPHacks.WireOrientationHack(this);

            viewModel = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            
            this.Loaded += ShopConfigurationPage_Loaded;
        }

        void ShopConfigurationPage_Loaded(object sender, RoutedEventArgs e)
        {
            appbar_up.Text = AppResources.upText;
            appbar_down.Text = AppResources.downText;
            appbar_renameCat.Text = AppResources.renameCatText;
            appbar_newShop.Text = AppResources.newShopText;
            appbar_renameShop.Text = AppResources.renameShopText;
            appbar_deleteShop.Text = AppResources.deleteShopText;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GoogleAnalytics.EasyTracker.GetTracker().SendView("ShopConfigurationPage");
        }

        private void ShopSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop == null)
            {
                appbar_renameCat.IsEnabled = false;
                appbar_down.IsEnabled = false;
                appbar_up.IsEnabled = false;
                appbar_deleteShop.IsEnabled = false;
                appbar_renameShop.IsEnabled = false;
                return;
            }
            appbar_renameCat.IsEnabled = shop.SelectedCategory != null;
            appbar_down.IsEnabled = shop.MoveDownCommand.CanExecute(null);
            appbar_up.IsEnabled = shop.MoveUpCommand.CanExecute(null);
            appbar_deleteShop.IsEnabled = viewModel.DeleteShopCommand.CanExecute(shop);
            foreach (Shop s in viewModel.Shops)
            {
                s.MoveUpCommand.CanExecuteChanged += MoveUpCommand_CanExecuteChanged;
                s.MoveDownCommand.CanExecuteChanged += MoveDownCommand_CanExecuteChanged;
            }
        }

        void MoveUpCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop == null) return;
            appbar_renameCat.IsEnabled = shop.SelectedCategory != null;
            appbar_up.IsEnabled = shop.MoveUpCommand.CanExecute(null);
        }

        void MoveDownCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop == null) return;
            appbar_renameCat.IsEnabled = shop.SelectedCategory != null;
            appbar_down.IsEnabled = shop.MoveDownCommand.CanExecute(null);
        }

        private void appbar_up_Click(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop == null) return;
            shop.MoveUpCommand.Execute(null);
        }

        private void appbar_down_Click(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop == null) return;
            shop.MoveDownCommand.Execute(null);
        }

        private void appbar_renameCat_Click(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop != null)
            {
                string urlString = String.Format("/View/RenameCategoryPage.xaml?CategoryName={0}", HttpUtility.UrlEncode(shop.SelectedCategory.CategoryName));
                NavigationService.Navigate(new Uri(urlString, UriKind.Relative));
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => {
                Shop shop = pivot.SelectedItem as Shop;
                if (shop != null)
                {
                    appbar_renameCat.IsEnabled = shop.SelectedCategory != null;
                    appbar_down.IsEnabled = shop.MoveDownCommand.CanExecute(null);
                    appbar_up.IsEnabled = shop.MoveUpCommand.CanExecute(null);
                }
            });
        }

        private void appbar_newShop_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AddShopPage.xaml", UriKind.Relative));
        }

        private void appbar_deleteShop_Click(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (viewModel.DeleteShopCommand.CanExecute(shop))
            {
                String message = String.Format(AppResources.deleteElementConfirmation, shop.Name);
                if (MessageBoxResult.OK == MessageBox.Show(message, AppResources.attentionText, MessageBoxButton.OKCancel))
                {
                    viewModel.DeleteShopCommand.Execute(shop);
                }
            }
            else
            {
                appbar_deleteShop.IsEnabled = false;
            }
        }

        private void appbar_renameShop_Click(object sender, EventArgs e)
        {
            Shop shop = pivot.SelectedItem as Shop;
            if (shop != null)
            {
                string urlString = String.Format("/View/RenameShopPage.xaml?ShopName={0}", HttpUtility.UrlEncode(shop.Name));
                NavigationService.Navigate(new Uri(urlString, UriKind.Relative));
            }

        }

    }
}