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
using GalaSoft.MvvmLight.Command;

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
        public class ShoppingListElement : ShoppingItem
        {
            private bool isSelected = false;
            public bool IsSelected
            {
                get
                {
                    return isSelected;
                }
                set
                {
                    if (value != isSelected)
                    {
                        isSelected = value;
                        RaisePropertyChanged("IsSelected");
                    }
                }
            }

            public ShoppingListElement()
                : base()
            {
            }

            public override void Update(SyncItemBase source)
            {
                ShoppingListElement si = source as ShoppingListElement;
                if (si != null)
                {
                    this.IsSelected= si.IsSelected;
                }
                base.Update(source);
            }

            public override SyncItemBase Clone()
            {
                ShoppingItem si = new ShoppingListElement();
                si.Update(this);
                return si;
            }


        }

        public delegate void ErrorNotificationHandler(string ErrorMessage);
        public event ErrorNotificationHandler ErrorNotification;

        /// <summary>
        /// The <see cref="ProductItems" /> property's name.
        /// </summary>
        public const string ProductItemsPropertyName = "ProductItems";

        private ObservableCollection<ShoppingListElement> productItems = new ObservableCollection<ShoppingListElement>();

        public ObservableCollection<ShoppingListElement> ProductItems
        {
            get
            {
                return productItems;
            }

            set
            {
                if (productItems == value)
                {
                    return;
                }

                productItems = value;
                RaisePropertyChanged(ProductItemsPropertyName);
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
                var result = (from item in this.ProductItems where item.IsOnShoppingList && !item.IsDeleted && item.caption.Contains(searchString) select item).ToList();
                var cv = CollectionViewSource.GetDefaultView(result);
                cv.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                cv.SortDescriptions.Add(new SortDescription("Caption", ListSortDirection.Ascending));
                cv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                if (this.CurrentShoppingItem != null)
                {
                    cv.MoveCurrentTo(CurrentShoppingItem);
                }
                return cv;
            }
        }

        public const string CurrentShoppingItemPropertyName = "CurrentShoppingItem";
        private ShoppingListElement currentShoppingItem = null;

        public ShoppingListElement CurrentShoppingItem
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
                
                if( currentShoppingItem != null )
                {
                    currentShoppingItem.IsSelected = false;
                }

                currentShoppingItem = value;
                
                currentShoppingItem.IsSelected = true;

                RaisePropertyChanged(CurrentShoppingItemPropertyName);
            }
        }

        FileSystemWatcher fileWatcher = null;
        Dispatcher loadDataDispatcher = null;

        public void LoadData()
        {
            loadDataDispatcher = Dispatcher.CurrentDispatcher;

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
            if (loadDataDispatcher != null)
            {
                loadDataDispatcher.BeginInvoke(new InvokeDelegate(LoadProductFile), productsfile);
            }
        }

        private delegate void InvokeDelegate(string argument);

        private void LoadProductFile(string productsfile)
        {
            using (FileStream fileStream = new FileStream(productsfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<ShoppingListElement>));
                List<ShoppingListElement> tmpList = jsonSerializer.ReadObject(fileStream) as List<ShoppingListElement>;
                if (tmpList != null)
                {
                    bool tmpListChanged = ListSync.SyncLists(this.ProductItems, tmpList);
                    //ProductItems.Clear();

                    //foreach (ShoppingListElement si in tmpList)
                    //{
                    //    ProductItems.Add(si);
                    //}
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
            this.IncreaseQuantityCommand = new RelayCommand<object>(this.IncreaseQuantity, this.CanIncreaseQuantity);
            this.DecreaseQuantityCommand = new RelayCommand<object>(this.DecreaseQuantity, this.CanDecreaseQuantity);
            this.AddToShoppingListCommand = new RelayCommand<object>(this.AddToList, this.CanAddToList);
            this.RemoveFromShoppingListCommand = new RelayCommand<object>(this.RemoveFromList, this.CanRemoveFromList);
        }

        public RelayCommand<object> IncreaseQuantityCommand { get; set; }

        private void IncreaseQuantity(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            if (pi.DefaultQuantity >= 900) return;

            int increment = 0;

            if (pi.DefaultQuantity >= 100)
            {
                increment = 100;
            }
            else if (pi.DefaultQuantity >= 10)
            {
                increment = 10;
            }
            else
            {
                increment = 1;
            }

            int rest = pi.DefaultQuantity % increment;
            if (rest == 0) pi.DefaultQuantity = pi.DefaultQuantity + increment;
            else pi.DefaultQuantity += (increment - rest);

            DecreaseQuantityCommand.RaiseCanExecuteChanged();
            IncreaseQuantityCommand.RaiseCanExecuteChanged();
        }

        private bool CanIncreaseQuantity(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return pi.DefaultQuantity < 900;
        }


        public RelayCommand<object> DecreaseQuantityCommand { get; set; }

        private void DecreaseQuantity(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            if (pi.DefaultQuantity <= 1) return;

            int oldVal = pi.DefaultQuantity;
            int decrement = 0;

            if (oldVal > 100)
            {
                decrement = 100;
            }
            else if (oldVal > 10)
            {
                decrement = 10;
            }
            else
            {
                decrement = 1;
            }

            int rest = oldVal % decrement;

            if (rest == 0) pi.DefaultQuantity -= decrement;
            else pi.DefaultQuantity -= rest;

            DecreaseQuantityCommand.RaiseCanExecuteChanged();
            IncreaseQuantityCommand.RaiseCanExecuteChanged();
        }

        private bool CanDecreaseQuantity(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return pi.DefaultQuantity > 1;
        }

        public RelayCommand<object> AddToShoppingListCommand { get; set; }

        private void AddToList(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (o != null)
            {
                item.IsOnShoppingList = true;
                RaisePropertyChanged("ShoppingList");
                this.CurrentShoppingItem = item as ShoppingListElement;
            }
        }

        private bool CanAddToList(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (o != null)
            {
                return !item.IsOnShoppingList;
            }
            return false;
        }

        public RelayCommand<object> RemoveFromShoppingListCommand { get; set; }

        private void RemoveFromList(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (o != null)
            {
                item.IsOnShoppingList = false;
                RaisePropertyChanged("ShoppingList");
                
            }
        }

        private bool CanRemoveFromList(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (o != null)
            {
                return item.IsOnShoppingList;
            }
            return false;
        }
    }
}