using Microsoft.Phone.Controls;
using System;
using Microsoft.Phone.Shell;
using System.Windows.Controls;
using OneShoppingList.Resources;
using System.Net;

namespace OneShoppingList.View
{
    /// <summary>
    /// Description for RenameShopPage.
    /// </summary>
    public partial class RenameShopPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        string oldShopName;

        /// <summary>
        /// Initializes a new instance of the AddShopPage class.
        /// </summary>
        public RenameShopPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);
            appbar_save = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            viewModel = this.DataContext as MainViewModel;
            this.Loaded += new System.Windows.RoutedEventHandler(RenameShopPage_Loaded);
        }

        void RenameShopPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            appbar_save.Text = AppResources.saveButtonText;
            textBox.Focus();
            textBox.SelectAll();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            appbar_save.IsEnabled = false;
            
            if (NavigationContext.QueryString.ContainsKey("ShopName"))
            {
                oldShopName = HttpUtility.UrlDecode(NavigationContext.QueryString["ShopName"]);
                ApplicationTitle.Text = String.Format(AppResources.renameStorePageTitle, oldShopName);
                textBox.Text = oldShopName;
            }
            GoogleAnalytics.EasyTracker.GetTracker().SendView("RenameShopPage");
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            appbar_save.IsEnabled = viewModel.IsValidShopName(textBox.Text);
        }

        private void appbar_save_Click(object sender, EventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ToolbarEvents", "ToolbarButton", "ToolbarButtonSaves", 0);
            viewModel.RenameShop(oldName: oldShopName, newName: textBox.Text);
            NavigationService.GoBack();
        }

    }
}