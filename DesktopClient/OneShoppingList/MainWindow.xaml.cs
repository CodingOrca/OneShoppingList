using System.Windows;
using OneShoppingList.ViewModel;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Text;

namespace OneShoppingList
{
    /// <summary>
    /// This application's main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            ViewModelLocator.MainStatic.ErrorNotification += new MainViewModel.ErrorNotificationHandler(MainStatic_ErrorNotification);
        }

        void MainStatic_ErrorNotification(string ErrorMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ERROR");
            sb.Append(ErrorMessage);
            MessageBox.Show(sb.ToString(), "ONE SHOPPING LIST", MessageBoxButton.OK);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.MainStatic.LoadData();
            TextBox searchTextBox = searchBox.Template.FindName("Text", searchBox) as TextBox;
            if (searchTextBox != null)
            {
                searchTextBox.Focus();
                searchBox.KeyUp += new KeyEventHandler(searchTextBox_KeyUp);
            }
        }

        void searchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (searchBox.SelectedItem != null)
                {
                    if (ViewModelLocator.MainStatic.AddToShoppingListCommand.CanExecute(searchBox.SelectedItem))
                    {
                        ViewModelLocator.MainStatic.AddToShoppingListCommand.Execute(searchBox.SelectedItem);
                    }
                    else
                    {
                        listBox.SelectedItem = searchBox.SelectedItem;
                    }
                }
                else
                {
                    AddNewProductItem();
                }
                searchBox.SelectedItem = null;
                searchBox.Text = "";
            }
        }

        private void AddNewProductItem()
        {
            if (searchBox.Text.Trim().Length >= 2)
            {
                MainViewModel.ShoppingListElement item = new MainViewModel.ShoppingListElement();
                item.Caption = searchBox.Text;
                item.Category = "Others";
                item.UnitSize = "pcs";
                ViewModelLocator.MainStatic.ProductItems.Add(item);
                ViewModelLocator.MainStatic.AddToShoppingListCommand.Execute(item);
                item.IsEditing = true;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.DragMove();

        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.IsGrouping)
            {
                if (listBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DelayedBringIntoView));
                else
                    listBox.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            }
            else
                listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (listBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;

            listBox.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DelayedBringIntoView));
        }

        private void DelayedBringIntoView()
        {
            var item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
            if (item != null)
                item.BringIntoView();
        }

        private void searchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void searchBox_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (searchBox.SelectedItem != null)
            {
                if (ViewModelLocator.MainStatic.AddToShoppingListCommand.CanExecute(searchBox.SelectedItem))
                {
                    ViewModelLocator.MainStatic.AddToShoppingListCommand.Execute(searchBox.SelectedItem);
                }
                else
                {
                    listBox.SelectedItem = searchBox.SelectedItem;
                }
                searchBox.SelectedItem = null;
                searchBox.Text = "";
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void navigateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://oneshoppinglist.wordpress.com");
            e.Handled = true;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox searchTextBox = searchBox.Template.FindName("Text", searchBox) as TextBox;
            if (searchTextBox != null)
            {
                searchTextBox.Clear();
                searchTextBox.Focus();
            }
        }

        private void addProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewProductItem();
        }

        private void searchBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Length >= 2)
            {
                addProductButton.IsEnabled = true;
            }
            else
            {
                addProductButton.IsEnabled = false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}