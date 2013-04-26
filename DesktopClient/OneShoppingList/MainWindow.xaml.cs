using System.Windows;
using OneShoppingList.ViewModel;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

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
                else if( !String.IsNullOrEmpty(searchBox.Text) )
                {
                    MainViewModel.ShoppingListElement item = new MainViewModel.ShoppingListElement();
                    item.Caption = searchBox.Text;
                    item.Category = "Others";
                    item.UnitSize = "pcs";
                    ViewModelLocator.MainStatic.ProductItems.Add(item);
                    ViewModelLocator.MainStatic.AddToShoppingListCommand.Execute(item);
                }
                searchBox.SelectedItem = null;
                searchBox.Text = "";
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
            System.Diagnostics.Process.Start("http://www.windowsphone.com/de-de/store/app/one-shopping-list/d2dbb41e-dd09-4621-b9f1-f6f5d5f7ab1b");
        }
    }
}