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
using TombstoneHelper;
using OneShoppingList.Model;
using LinqToVisualTree;
using System.Windows.Data;
using OneShoppingList.ViewModel;
using System.Windows.Threading;
using MetroInMotionUtils;
using Microsoft.Phone.Shell;
using OneShoppingList.Resources;
using System.Text;
using Microsoft.Phone.Tasks;

namespace OneShoppingList
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        ViewModelLocator locator;

        public MainPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);

            appBar = this.ApplicationBar as ApplicationBar;

            appbar_sendEmail = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            appbar_add = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            appbar_favorites = this.ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            appbar_sync = this.ApplicationBar.Buttons[3] as ApplicationBarIconButton;

            appbar_clearList = this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            appbar_shopsConfig = this.ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            appbar_settings = this.ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;

            locator = App.Current.Resources["Locator"] as ViewModelLocator;

            viewModel = locator.Main;

            viewModel.SyncCommand.CanExecuteChanged += SyncCommand_CanExecuteChanged;
            appbar_sync.IsEnabled = viewModel.SyncCommand.CanExecute(null);
            viewModel.UnsortedShoppingList.CollectionChanged += DataCollectionChanged;
            viewModel.Shops.CollectionChanged += DataCollectionChanged;
            viewModel.AllItemsViewModel.ItemsView.CollectionChanged += DataCollectionChanged;
            
            if (locator.Settings.IsUserKnown && locator.Settings.SyncEnabled)
            {
                viewModel.SyncCommand.Execute(null);
            }

            if (locator.Settings.LastShoppingStore != -1 && locator.Settings.LastShoppingStore < viewModel.Shops.Count)
            {
                viewModel.CurrentShop = viewModel.Shops[locator.Settings.LastShoppingStore];
            }

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.BackKeyPress += MainPage_BackKeyPress;
        }

        void DataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool shops = viewModel.Shops.Count > 0;
            bool items = viewModel.UnsortedShoppingList.Count > 0;
            bool products = viewModel.AllItemsViewModel.ItemsView.Count > 0;

            appbar_sendEmail.IsEnabled = shops && items;
            appbar_add.IsEnabled = shops;
            appbar_favorites.IsEnabled = shops && products;
            appbar_clearList.IsEnabled = items;
            appbar_shopsConfig.IsEnabled = shops;
        }

        private bool isPageClosing = false;
        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                e.Cancel = true;
                return;
            }
            this.IsEnabled = false;
            appBar.IsVisible = false;
            this.BackKeyPress -= MainPage_BackKeyPress;

            ViewModelLocator locator = App.Current.Resources["Locator"] as ViewModelLocator;

            locator.Main.SaveData();

            if (!isPageClosing)
            {
                isPageClosing = true;
                if (locator.Settings.IsUserKnown && locator.Settings.SyncEnabled)
                {
                    if (locator.Main.SyncCommand.CanExecute(null))
                    {
                        int localChanges = DataLocator.Current.ProductItems.Count(pi => pi.TimeStamp > locator.Main.SyncHandler.LastSyncTime);
                        int localStoreChanges = DataLocator.Current.Shops.Count(s => s.TimeStamp > locator.Main.SyncHandler.LastSyncTime);
                        if (localChanges > 0 || localStoreChanges > 0)
                        {
                            locator.Main.SyncCommand.Execute(null);
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            appbar_add.Text = AppResources.addButtonText;
            appbar_favorites.Text = AppResources.favButtonText;
            appbar_sync.Text = AppResources.syncButtonText;
            appbar_clearList.Text = AppResources.clearListMenu;
            appbar_shopsConfig.Text = AppResources.shopsConfigMenu;
            appbar_settings.Text = AppResources.settingsMenu;
            appbar_sendEmail.Text = AppResources.sendEmailMenu;

            this.DataCollectionChanged(null, null);

            //if (locator.Settings.IsFirstStart)
            //{
            //    locator.Settings.IsFirstStart = false;

            //    if (!locator.Settings.IsUserKnown)
            //    {
            //        NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
            //    }
            //}

        }

        void SyncCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            appbar_sync.IsEnabled = viewModel.SyncCommand.CanExecute(null);
            if (isPageClosing && appbar_sync.IsEnabled)
            {
                this.IsEnabled = true;
                NavigationService.GoBack();
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // viewModel.SaveData();
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // TODO:
            // this.RestoreState();
        }

        void skyDriveHandler_LoginCompleted(string result)
        {
            //skyDriveHandler.RefreshTheToken(skyDriveHandler.RefreshToken);
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            //SetDownTransition();
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }

        private void Einkaufskorb_Click(object sender, EventArgs e)
        {
            //SetUpTransition();
            NavigationService.Navigate(new Uri("/View/BasketPage.xaml", UriKind.Relative));
            // skyDriveHandler.StartSync();
        }

        private void About_Click(object sender, EventArgs e)
        {
            //SetDownTransition();
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        private void ShopsConfiguration_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/ShopConfigurationPage.xaml", UriKind.Relative));
        }

        private ItemFlyInAndOutAnimations _flyOutAnimation = new ItemFlyInAndOutAnimations(0,900);

        private void ToShoppingBasket_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement item = sender as FrameworkElement;
            if (item != null)
            {
                item = item.Parent as FrameworkElement;
                _flyOutAnimation.ItemFlyOut(item, () =>
                {
                    viewModel.ToggleInShoppingBasket.Execute(item.DataContext);
                });
            }
        }

        private void longListSelector_ScrollingStarted(object sender, EventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;
            if (lls.SelectedItem != null)
            {
                lls.SelectedItem = null;
            }
            this.Focus();
            this.ApplicationBar.IsMenuEnabled = false;
            appbar_sendEmail.IsEnabled = false;
            appbar_add.IsEnabled = false;
            appbar_favorites.IsEnabled = false;
            appbar_sync.IsEnabled = false;
        }

        private void longListSelector_ScrollingCompleted(object sender, EventArgs e)
        {
            this.ApplicationBar.IsMenuEnabled = true;
            DataCollectionChanged(sender, null);
            appbar_sync.IsEnabled = viewModel.SyncCommand.CanExecute(null);
        }

        private void appbarFavorits_Click(object sender, EventArgs e)
        {
            //SetHorizontalTransition();
            NavigationService.Navigate(new Uri("/View/EditPage.xaml", UriKind.Relative));
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

            ContentPresenter cp = fe.Ancestors<ContentPresenter>().FirstOrDefault() as ContentPresenter;
            double oldOpacity = cp.Opacity;
            cp.Opacity = 1.0;

            Brush oldBrush = fe.Background;
            fe.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            currentContextMenu.Closed += new RoutedEventHandler((snd, ea) =>
            {
                fe.Background = oldBrush;
                cp.Opacity = oldOpacity;
                currentContextMenu = null;
            });
        }

        private void RemoveButton_Click(object sender, EventArgs e)
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

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenu cm = ContextMenuService.GetContextMenu(sender as Grid);
            cm.IsOpen = true;
        }

        private void LongListSelector_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        {
            //Hold a reference to the active long lls selector.
            LongListSelector longListSelector = sender as LongListSelector;

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

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/ShopSelectionPage.xaml", UriKind.Relative));
        }

        Dictionary<int, List<FrameworkElement>> linkedItems = new Dictionary<int, List<FrameworkElement>>();
        private void longListSelector_Link(object sender, LinkUnlinkEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            ShoppingItem item = e.ContentPresenter.Content as ShoppingItem;

            if (item == null)
            {
                return;
            }

            Shop shop = lls.DataContext as Shop;

            if (item != null)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    Grid grid = VisualTreeHelper.GetChild(e.ContentPresenter, 0) as Grid;
                    if (grid != null)
                    {
                        PivotItem pivotItem = e.ContentPresenter.Ancestors<PivotItem>().SingleOrDefault() as PivotItem;
                        if (pivotItem != null)
                        {
                            string currentShop = (pivotItem.DataContext as Shop).Name;

                            TextBlock preferredShop = grid.Descendants<TextBlock>().Where(d => (d as TextBlock).Name == "PreferredShop").FirstOrDefault() as TextBlock;
                            if (preferredShop != null)
                            {
                                if (preferredShop.Text == currentShop)
                                {
                                    preferredShop.Foreground = App.Current.Resources["PhoneAccentBrush"] as Brush;
                                }
                                else
                                {
                                    preferredShop.Foreground = App.Current.Resources["PhoneSubtleBrush"] as Brush;
                                }
                            }
                        }
                    }
                }
                );
            }
        }

        private void longListSelector_Unlink(object sender, LinkUnlinkEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                currentContextMenu.IsOpen = false;
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;
            lls.SelectedItem = null;
        }

        private void appbar_clearList_Click(object sender, EventArgs e)
        {
            viewModel.ClearShoppingList();
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                currentContextMenu.IsOpen = false;
            }
            this.locator.Settings.LastShoppingStore = pivot.SelectedIndex;
        }

        private void ShoppingItemContextMenu_OnAddFavorite(object sender, EventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (cm != null)
            {
                cm.IsOpen = false;
            }
        }

        private void ShoppingItemContextMenu_OnRemoveFavorite(object sender, EventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (cm != null)
            {
                cm.IsOpen = false;
            }
        }

        private void appbar_sync_Click(object sender, EventArgs e)
        {
            if (!locator.Settings.IsUserKnown)
            {
                Settings_Click(sender, e);
            }
            else if (viewModel.SyncCommand.CanExecute(null))
            {
                viewModel.SyncCommand.Execute(null);
            }
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void appbar_sendEmail_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(AppResources.sortedFor + viewModel.CurrentShop.Name.ToUpper());
            sb.AppendLine("------------------------------------");
            foreach (var category in viewModel.CurrentShop.ShoppingListViewModel.GroupsView)
            {
                sb.AppendLine(category.Key.ToUpper());
                foreach (var item in category)
                {
                    sb.AppendFormat(" {0} {1} {2}", item.DefaultQuantity, item.UnitSize, item.Caption);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            EmailComposeTask task = new EmailComposeTask();
            task.Body = sb.ToString();
            task.Subject = AppResources.emailSubject;
            task.Show();
        }

        private void appbar_about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            string itemsfilename = "";
            string shopsfilename = "";
            switch (picker.SelectedIndex)
            {
                case 0:
                    itemsfilename = "Content/DE-DefaultItemList.json";
                    shopsfilename = "Content/DE-DefaultShopList.json";
                    break;
                case 1:
                    itemsfilename = "Content/EN-DefaultItemList.json";
                    shopsfilename = "Content/EN-DefaultShopList.json";
                    break;
            }
            DataLocator.Current.LoadDefaultData(itemsfilename, shopsfilename);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DataLocator.Current.ProductItems.Clear();
            DataLocator.Current.Shops.Clear();
        }

        private void syncButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml?autoreturn=true", UriKind.Relative));
        }

        private void addPathButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AddShopPage.xaml", UriKind.Relative));
        }

    }
}