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
using OneShoppingList.ViewModel;
using System.Windows.Threading;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using OneShoppingList.Resources;

namespace OneShoppingList.View
{
    public partial class AddProductItemPage : PhoneApplicationPage
    {
        ViewModelLocator locator;
        AddProductItemViewModel viewModel;

        public AddProductItemPage()
        {
            InitializeComponent();
            Framework.WPHacks.WireOrientationHack(this);

            appbar_cancel = this.ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            appbar_save = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            appbar_delete = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            viewModel = (locator = App.Current.Resources["Locator"] as ViewModelLocator).AddProductItem;
            viewModel.Init();
            this.Loaded += AddProductItemPage_Loaded;
        }

        void AddProductItemPage_Loaded(object sender, RoutedEventArgs e)
        {
            productNameBox.Focus();
            appbar_cancel.Text = AppResources.cancelButtonText;
            appbar_delete.Text = AppResources.deleteButtonText;
            appbar_save.Text = AppResources.saveButtonText;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            appbar_save.IsEnabled = false;
            BindingExpression be = quantityBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();

            viewModel.Save();

            if (NavigationContext.QueryString.ContainsKey("Item")) // Edit Mode for one target, nothing to do on this page anymore
            {
                NavigationService.GoBack();
            }
            else
            {
                viewModel.ProductName = "";
                productNameBox.Focus();

                progressIndicator.IsIndeterminate = true;
                progressIndicator.IsVisible = true;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();

                appbar_save.IsEnabled = false;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop();

            progressIndicator.IsIndeterminate = false;
            progressIndicator.IsVisible = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            appbar_save.IsEnabled = false;

            if (NavigationContext.QueryString.ContainsKey("Item"))
            {
                // remove bindings to autocomplete handling for the productKey AutoCompleteBox:
                productNameBox.ClearValue(AutoCompleteBox.ItemsSourceProperty);
                productNameBox.ClearValue(AutoCompleteBox.SelectedItemProperty);

                string productKey = NavigationContext.QueryString["Item"];
                viewModel.SetEditItem(Guid.Parse(productKey));

                if (viewModel.EditItem != null)
                {
                    this.productCategoryBox.Text = viewModel.ProductCategory;
                    this.shopPicker.Text = viewModel.PreferredShop;
                    this.unitSizesBox.Text = viewModel.UnitSize;
                    this.quantityBox.Text = viewModel.Quantity.ToString();

                    titlePanel.Text = viewModel.ProductName.ToUpper();
                    appbar_delete.IsEnabled = true;
                }
            }
            else
            {
                viewModel.ProductCategory = AppResources.defaultCategory;
                viewModel.PreferredShop = locator.Main.CurrentShop != null ? locator.Main.CurrentShop.Name : "";
                viewModel.UnitSize = AppResources.defaultUnit;
                viewModel.Quantity = 1;
            }
        }

        private void AutoCompleteBox_DropDownOpening(object sender, RoutedPropertyChangingEventArgs<bool> e)
        {
            
            e.Cancel = focusedObject != sender;
        }

        private object focusedObject;
        private void AutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            focusedObject = sender;
        }

        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            focusedObject = null;
        }

        // We use this only for enabling / disabling the ApplicationBar Buttons!
        // So the only check we do is to ensure the Product name is not empty.
        private void AutoCompleteBox_TextChanged(object sender, RoutedEventArgs e)
        {
            appbar_save.IsEnabled = (productNameBox.Text.Length > 0);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            NavigationService.GoBack();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            viewModel.DeleteEditItem();
            NavigationService.GoBack();
        }

    }
}