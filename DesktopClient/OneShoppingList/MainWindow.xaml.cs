using System.Windows;
using OneShoppingList.ViewModel;
using System.Windows.Data;
using System.Windows.Controls;

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
            searchBox.Focus();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void searchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            BindingExpression be = searchBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
    }
}