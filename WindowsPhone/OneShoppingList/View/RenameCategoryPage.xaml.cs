using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using OneShoppingList.Resources;
using System.Net;

namespace OneShoppingList.View
{
    /// <summary>
    /// Description for RenameCategoryPage.
    /// </summary>
    public partial class RenameCategoryPage : PhoneApplicationPage
    {
        MainViewModel viewModel;
        string oldCategoryName;

        /// <summary>
        /// Initializes a new instance of the RenameCategoryPage class.
        /// </summary>
        public RenameCategoryPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);
            appbar_save = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            viewModel = this.DataContext as MainViewModel;
            this.Loaded += new RoutedEventHandler(RenameCategoryPage_Loaded);
        }

        void RenameCategoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            appbar_save.Text = AppResources.saveButtonText;
            textBox.Focus();
            textBox.SelectAll();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            appbar_save.IsEnabled = false;

            if (NavigationContext.QueryString.ContainsKey("CategoryName"))
            {
                oldCategoryName = HttpUtility.UrlDecode(NavigationContext.QueryString["CategoryName"]);
                ApplicationTitle.Text = String.Format(AppResources.renameCategoryPageTitle, oldCategoryName);
                textBox.Text = oldCategoryName;
            }

            GoogleAnalytics.EasyTracker.GetTracker().SendView("RenameCategoryPage");

        }
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            appbar_save.IsEnabled = viewModel.IsValidCategoryName(oldName: oldCategoryName, newName: textBox.Text);
        }

        private void appbar_save_Click(object sender, EventArgs e)
        {
            bool doRename = true;
            if (viewModel.CategoryExists(textBox.Text))
            {
                string message = String.Format(AppResources.categoryMergeDialog, textBox.Text);
                if (MessageBoxResult.OK != MessageBox.Show(message, AppResources.attentionText, MessageBoxButton.OKCancel))
                {
                    doRename = false;
                }
            }
            if (doRename)
            {
                viewModel.RenameCategory(oldName: oldCategoryName, newName: textBox.Text);
                NavigationService.GoBack();
            }
        }
    }
}