using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows.Controls;
using OneShoppingList.Resources;

namespace OneShoppingList.View
{
    /// <summary>
    /// Description for AddShopPage.
    /// </summary>
    public partial class AddShopPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        /// <summary>
        /// Initializes a new instance of the AddShopPage class.
        /// </summary>
        public AddShopPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);
            appbar_save = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            viewModel = this.DataContext as MainViewModel;
            this.Loaded += new System.Windows.RoutedEventHandler(AddShopPage_Loaded);
        }

        void AddShopPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            textBox.Focus();
            appbar_save.Text = AppResources.saveButtonText;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            appbar_save.IsEnabled = viewModel.IsValidShopName(textBox.Text);
        }

        private void appbar_save_Click(object sender, EventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ToolbarEvents", "ToolbarButton", "ToolbarButtonSaves", 0);
            viewModel.AddShopCommand.Execute(textBox.Text);
            NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("AddShopPage");
        }
    }
}