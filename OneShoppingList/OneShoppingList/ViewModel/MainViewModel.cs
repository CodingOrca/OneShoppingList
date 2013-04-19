using GalaSoft.MvvmLight;
using OneShoppingList.Model;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System;
using System.Collections.ObjectModel;

namespace OneShoppingList.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public string Welcome
        {
            get
            {
                return "Welcome to MVVM Light";
            }
        }

        /// <summary>
        /// The <see cref="ShoppingItems" /> property's name.
        /// </summary>
        public const string ShoppingItemsPropertyName = "ShoppingItems";

        private ObservableCollection<ShoppingItem> shoppingItems = new ObservableCollection<ShoppingItem>();

        public ObservableCollection<ShoppingItem> ShoppingItems
        {
            get
            {
                return shoppingItems;
            }

            set
            {
                if (shoppingItems == value)
                {
                    return;
                }

                shoppingItems = value;
                RaisePropertyChanged(ShoppingItemsPropertyName);
            }
        }

        public const string CurrentShoppingItemPropertyName = "CurrentShoppingItem";
        private ShoppingItem currentShoppingItem = null;

        public ShoppingItem CurrentShoppingItem
        {
            get
            {
                return currentShoppingItem;
            }

            set
            {
                if (currentShoppingItem == value)
                {
                    return;
                }

                currentShoppingItem = value;
                RaisePropertyChanged(CurrentShoppingItemPropertyName);
            }
        }

        public void LoadData()
        {
            string filename = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @"SkyDrive\AppData\OneFamily\ShoppingList\OneShoppingList.txt");
            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                List<ShoppingItem> tmpList = jsonSerializer.ReadObject(fileStream) as List<ShoppingItem>;
                if (tmpList != null)
                {
                    ShoppingItems.Clear();
                    foreach (ShoppingItem si in tmpList)
                    {
                        ShoppingItems.Add(si);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}