using GalaSoft.MvvmLight;
using OneShoppingList.Model;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;

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
        public delegate void ErrorNotificationHandler(string ErrorMessage);
        public event ErrorNotificationHandler ErrorNotification;

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

        private string searchString = "";
        public string SearchString
        {
            get
            {
                return searchString;
            }
            set
            {
                if (searchString != value)
                {
                    searchString = value;
                    RaisePropertyChanged("searchString");
                    RaisePropertyChanged("ShoppingList");
                }
            }
        }

        public ICollectionView ShoppingList
        {
            get
            {
                var result = (from item in this.ShoppingItems where item.caption.Contains(searchString) select item).ToList();
                this.CurrentShoppingItem = result[0];
                var cv = CollectionViewSource.GetDefaultView(result);
                cv.GroupDescriptions.Add(new PropertyGroupDescription("category"));
                return cv;
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

        FileSystemWatcher fileWatcher = null;
        public void LoadData()
        {
            string homedir = Environment.GetEnvironmentVariable("USERPROFILE");
            string skydrivedir = Path.Combine(homedir, @"SkyDrive");
            string oneshoppinghome = Path.Combine(skydrivedir, @"AppData\OneFamily\ShoppingList");
            string productsfile = Path.Combine(oneshoppinghome, @"OneShoppingList.txt");

            if (!Directory.Exists(homedir))
            {
                // this should never happen
                NotifyError(homedir);
                return;
            }
            else if (!Directory.Exists(skydrivedir))
            {
                NotifyError("SkyDrive not installed. Download from here: http://windows.microsoft.com/en-US/skydrive/download");
                return;
            }
            else
            {
                if (!Directory.Exists(oneshoppinghome))
                {
                    Directory.CreateDirectory(oneshoppinghome);
                }
                if (File.Exists(productsfile))
                {
                    ReloadProductsFile(productsfile);
                }
            }

            fileWatcher = new FileSystemWatcher(oneshoppinghome, "*.*");
            fileWatcher.Changed += fileWatcher_Changed;
            fileWatcher.Created += fileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
        }

        void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if( e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                ReloadProductsFile(e.FullPath);
            }
        }

        private void ReloadProductsFile(string productsfile)
        {
            using (FileStream fileStream = new FileStream(productsfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
            RaisePropertyChanged("ShoppingList");
        }

        private void NotifyError(string errorstring)
        {
            if (ErrorNotification != null)
            {
                ErrorNotification(String.Format("User profile location take from environment variable USERPROFILE not found: {0}", errorstring));
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