﻿using System;
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
using LinqToVisualTree;
using OneShoppingList.Model;
using OneShoppingList.ViewModel;
using Microsoft.Phone.Shell;
using OneShoppingList.Resources;

namespace OneShoppingList.View
{
    public partial class EditPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        ViewModelLocator locator;

        public EditPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this); 
            viewModel = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            addButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            appbar_clearList = this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            appbar_shopsConfig = this.ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            appbar_settings = this.ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            locator = App.Current.Resources["Locator"] as ViewModelLocator;

            this.Loaded += new RoutedEventHandler(EditPage_Loaded);

            if (locator.Settings.LastFavoritesPivotItem != -1 && locator.Settings.LastFavoritesPivotItem < 3)
            {
                pivot.SelectedIndex = locator.Settings.LastFavoritesPivotItem;
            }
        }

        void EditPage_Loaded(object sender, RoutedEventArgs e)
        {
            addButton.Text = AppResources.addButtonText;
            appbar_settings.Text = AppResources.settingsMenu;
            
            appbar_clearList.Text = AppResources.clearListMenu;
            appbar_shopsConfig.Text = AppResources.shopsConfigMenu;
            appbar_settings.Text = AppResources.settingsMenu;

            isInitialized = true;
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

        private void longListSelector_ScrollingStarted(object sender, EventArgs e)
        {
            LongListSelector longListSelector = sender as LongListSelector;
            if (longListSelector.SelectedItem != null)
            {
                longListSelector.SelectedItem = null;
            }
            this.Focus();
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }

        private void ShopsConfiguration_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/ShopConfigurationPage.xaml", UriKind.Relative));
        }

        private void appbar_clearList_Click(object sender, EventArgs e)
        {
            if (MessageBoxResult.OK == MessageBox.Show(AppResources.deleteListConfirmation, AppResources.attentionText, MessageBoxButton.OKCancel))
            {
                viewModel.ClearShoppingList();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AddProductItemPage.xaml", UriKind.Relative));
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
            IsCheckBoxClicked = true;
            // this is to avoid the tap causing the popup to appear, when the checkbox was clicked.
            Dispatcher.BeginInvoke(() => IsCheckBoxClicked = false);
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsCheckBoxClicked) return;
            ContextMenu cm  = ContextMenuService.GetContextMenu(sender as Grid);
            cm.IsOpen = true;
        }

        private void LongListSelector_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        {
            LongListSelector longListSelector = sender as LongListSelector;

            //Hold a reference to the active long lls selector.
            longListSelector = sender as LongListSelector;

            //Construct and begin a swivel animation to pop in the group view.
            IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            Storyboard _swivelShow = new Storyboard();
            ItemsControl groupItems = e.ItemsControl;

            foreach (var item in groupItems.Items)
            {
                UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (container != null)
                {
                    Button content = VisualTreeHelper.GetChild(container, 0) as Button;
                    if (content != null)
                    {
                        DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

                        EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
                        showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
                        showKeyFrame1.Value = -90;
                        showKeyFrame1.EasingFunction = quadraticEase;

                        EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
                        showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(300);
                        showKeyFrame2.Value = 0;
                        showKeyFrame2.EasingFunction = quadraticEase;

                        showAnimation.KeyFrames.Add(showKeyFrame1);
                        showAnimation.KeyFrames.Add(showKeyFrame2);

                        Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
                        Storyboard.SetTarget(showAnimation, content.Projection);

                        _swivelShow.Children.Add(showAnimation);
                    }
                }
            }

            _swivelShow.Begin();
        }

        private void LongListSelector_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        {
            LongListSelector longListSelector = sender as LongListSelector;

            //Cancelling automatic closing and scrolling to do it manually.
            e.Cancel = true;

            //Dispatch the swivel animation for performance on the UI thread.
            Dispatcher.BeginInvoke(() =>
            {
                //Construct and begin a swivel animation to pop out the group view.
                IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                Storyboard _swivelHide = new Storyboard();
                ItemsControl groupItems = e.ItemsControl;

                foreach (var item in groupItems.Items)
                {
                    UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                    if (container != null)
                    {
                        Button content = VisualTreeHelper.GetChild(container, 0) as Button;
                        if (content != null)
                        {
                            DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

                            EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
                            showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
                            showKeyFrame1.Value = 0;
                            showKeyFrame1.EasingFunction = quadraticEase;

                            EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
                            showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(125);
                            showKeyFrame2.Value = 90;
                            showKeyFrame2.EasingFunction = quadraticEase;

                            showAnimation.KeyFrames.Add(showKeyFrame1);
                            showAnimation.KeyFrames.Add(showKeyFrame2);

                            Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
                            Storyboard.SetTarget(showAnimation, content.Projection);

                            _swivelHide.Children.Add(showAnimation);
                        }
                    }
                }

                _swivelHide.Completed += (s1, e1) =>
                {
                    if (e.SelectedGroup != null)
                    {
                        longListSelector.ScrollToGroup(e.SelectedGroup);
                    }
                    //Close group view.
                    if (longListSelector != null)
                    {
                        longListSelector.CloseGroupView();
                    }
                };
                _swivelHide.Begin();

            });
        }


        private void PhoneApplicationPage_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.DataContext == null)
            {
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        if (this.DataContext == null)
                        {
                            this.DataContext = viewModel;
                        }
                    }
                    );
            }
        }

        private void longListSelector_Link(object sender, LinkUnlinkEventArgs e)
        {
        }

        private void longListSelector_Unlink(object sender, LinkUnlinkEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                Grid grid = currentContextMenu.Owner as Grid;
                object parentLLS = grid.Ancestors<LongListSelector>().SingleOrDefault();
                if (parentLLS == sender || parentLLS == null)
                {
                    currentContextMenu.IsOpen = false;
                }
            }
        }

        private bool isInitialized = false;
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                this.locator.Settings.LastFavoritesPivotItem = pivot.SelectedIndex;
            }
        }
 
       private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

    }
}