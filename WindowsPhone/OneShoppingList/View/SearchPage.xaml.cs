using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using OneShoppingList.Model;
using System.Windows.Media;

namespace OneShoppingList.View
{
    public partial class SearchPage : PhoneApplicationPage
    {
        MainViewModel viewModel = null;
        public SearchPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);
            viewModel = this.DataContext as MainViewModel;
            viewModel.SearchString = "";
            this.Loaded += SearchPage_Loaded;
        }

        void SearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.searchBox.Focus();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                currentContextMenu.IsOpen = false;
            }
            // TODO:
            // this.SaveState(e);
        }

        ContextMenu currentContextMenu = null;
        private void menu_Opened(object sender, RoutedEventArgs e)
        {
            currentContextMenu = sender as ContextMenu;
            Grid fe = currentContextMenu.Owner as Grid;

            currentContextMenu.DataContext = fe.DataContext;

            Brush oldBrush = fe.Background;
            fe.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            currentContextMenu.Closed += new RoutedEventHandler((snd, ea) =>
            {
                fe.Background = oldBrush;
                currentContextMenu = null;
            });
        }

        private void CloseContextMenu(object sender, EventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (cm != null)
            {
                cm.IsOpen = false;
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (cm != null)
            {
                cm.IsOpen = false;
            }
            ShoppingItem pi = cm.DataContext as ShoppingItem;
            string param = String.Format("/View/AddProductItemPage.xaml?Mode=Edit&Item={0}", pi.Key);
            NavigationService.Navigate(new Uri(param, UriKind.Relative));
        }

        private bool IsCheckBoxClicked = false;
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ListEvents", "CheckBox", "CheckBoxItemToList", 0);
            IsCheckBoxClicked = true;
            // this is to avoid the tap causing the popup to appear, when the checkbox was clicked.
            Dispatcher.BeginInvoke(() => IsCheckBoxClicked = false);
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsCheckBoxClicked || searchBoxHasFocus) return;
            ContextMenu cm = ContextMenuService.GetContextMenu(sender as Grid);
            cm.IsOpen = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("SearchPage");
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression be = searchBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        private bool searchBoxHasFocus = false;
        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            searchBoxHasFocus = false;
        }

        private void searchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            searchBoxHasFocus = true;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("NavigationEvents", "AddProductButton", "AddProductButton", 0);
            NavigationService.Navigate(new Uri("/View/AddProductItemPage.xaml", UriKind.Relative));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Focus();
        }
    }
}