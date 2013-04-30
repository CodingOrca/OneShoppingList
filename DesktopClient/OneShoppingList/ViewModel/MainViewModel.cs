using GalaSoft.MvvmLight;
using OneShoppingList.Model;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Collections;
using System.Runtime.Serialization;

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

            Productsfile = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @"SkyDrive\AppData\OneFamily\ShoppingList\OneShoppingList.txt");

            this.IncreaseQuantityCommand = new RelayCommand<object>(this.IncreaseQuantity, this.CanIncreaseQuantity);
            this.DecreaseQuantityCommand = new RelayCommand<object>(this.DecreaseQuantity, this.CanDecreaseQuantity);
            this.AddToShoppingListCommand = new RelayCommand<object>(this.AddToList, this.CanAddToList);
            this.RemoveFromShoppingListCommand = new RelayCommand<object>(this.RemoveFromList, this.CanRemoveFromList);
            this.EditItemCommand = new RelayCommand<object>(this.StartEditing, this.CanStartEditing);
            this.SaveItemCommand = new RelayCommand<object>(this.CloseEditing, this.CanCloseEditing);
            this.SaveCommand = new RelayCommand<object>(this.SaveAll, this.CanSaveAll);
            this.DeleteItemCommand = new RelayCommand<object>(this.DeleteItem, this.CanDeleteItem);
        }

        [DataContract]
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

            private bool isEditing = false;
            public bool IsEditing
            {
                get
                {
                    return isEditing;
                }
                set
                {
                    if (value != isEditing)
                    {
                        isEditing = value;
                        RaisePropertyChanged("IsEditing");
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

        public bool IsConnected
        {
            get
            {
                return IsSkyDriveRunning && DoesProductFileExists;
            }
        }

        private bool isDirty = false;
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                if (value == isDirty)
                {
                    return;
                }
                isDirty = value;
                RaisePropertyChanged("IsDirty");
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string applicationState = "Not connected";
        public string ApplicationState
        {
            get
            {
                return applicationState;
            }
            set
            {
                if (value != applicationState)
                {
                    applicationState = value;
                    RaisePropertyChanged("ApplicationState");
                }
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

        public IEnumerable VisibleProducts
        {
            get
            {
                return from item in this.ProductItems where !item.IsDeleted select item;
            }
        }

        private ICollectionView shoppingList = null;
        public ICollectionView ShoppingList
        {
            get
            {
                if (shoppingList == null)
                {
                    shoppingList = CollectionViewSource.GetDefaultView(this.ProductItems);
                    shoppingList.Filter = IsShoppingItem;
                    shoppingList.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                    shoppingList.SortDescriptions.Add(new SortDescription("Caption", ListSortDirection.Ascending));
                    shoppingList.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                    shoppingList.CurrentChanged += new EventHandler(shoppingList_CurrentChanged);
                    if (this.currentShoppingItem != null)
                    {
                        shoppingList.MoveCurrentTo(currentShoppingItem);
                    }
                    else
                    {
                        bool success = shoppingList.MoveCurrentToFirst();
                    }
                }
                return shoppingList;
            }
        }

        private ShoppingListElement currentShoppingItem = null;
        void shoppingList_CurrentChanged(object sender, EventArgs e)
        {
            if (currentShoppingItem != null)
            {
                if (currentShoppingItem.IsEditing)
                {
                    SaveItemCommand.Execute(currentShoppingItem);
                }
                currentShoppingItem.IsSelected = false;
            }

            currentShoppingItem = shoppingList.CurrentItem as ShoppingListElement;

            if (currentShoppingItem != null)
            {
                currentShoppingItem.IsSelected = true;
            }

        }

        bool IsShoppingItem(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (item == null) return false;
            return item.IsOnShoppingList && !item.IsDeleted;
        }

        FileSystemWatcher fileWatcher = null;
        Dispatcher loadDataDispatcher = null;

        public string Productsfile { get; private set; }

        public void LoadData()
        {
            loadDataDispatcher = Dispatcher.CurrentDispatcher;

            SyncProductFile();

            string dirpath = Path.GetDirectoryName(Productsfile);
            if (Directory.Exists(dirpath))
            {
                fileWatcher = new FileSystemWatcher(dirpath, "*.*");
                fileWatcher.Changed += fileWatcher_Changed;
                fileWatcher.Created += fileWatcher_Changed;
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "OneShoppingList.txt")
            {
                if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
                {
                    if (loadDataDispatcher != null)
                    {
                        loadDataDispatcher.BeginInvoke(new InvokeDelegate(SyncProductFile));
                    }
                }
            }
        }

        private delegate void InvokeDelegate();

        public void SyncProductFile()
        {
            if (!File.Exists(Productsfile))
            {
                this.ApplicationState = "Not connected";
                return;
            }

            List<ShoppingListElement> tmpList = LoadProductFile(Productsfile);
            
            if (tmpList != null)
            {
                bool tmpListChanged = ListSync.SyncLists(this.ProductItems, tmpList);
                if (tmpListChanged)
                {
                    fileWatcher.EnableRaisingEvents = false;
                    SaveProductFile(Productsfile, tmpList);
                    fileWatcher.EnableRaisingEvents = true;
                }
            }

            if (shoppingList.CurrentItem == null)
            {
                shoppingList.MoveCurrentToFirst();
            }
            ShoppingList.Refresh();
            RaisePropertyChanged("VisibleProducts");
        }

        private List<ShoppingListElement> LoadProductFile(string productsfile)
        {
            List<ShoppingListElement> tmpList = null;
            try
            {
                using (FileStream fileStream = new FileStream(productsfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<ShoppingListElement>));
                    tmpList = jsonSerializer.ReadObject(fileStream) as List<ShoppingListElement>;
                }
                IsDirty = false;
                this.ApplicationState = IsSkyDriveRunning ? "Connected" : "Not connected";
            }
            catch (Exception)
            {
                this.ApplicationState = "ERROR: Could not read your data from SkyDrive";
            }
            return tmpList;
        }

        private void SaveProductFile(string productsfile, List<ShoppingListElement> tmpList)
        {
            try
            {
                using (FileStream fileStream = new FileStream(productsfile, FileMode.Truncate, FileAccess.Write, FileShare.None))
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<ShoppingListElement>));
                    jsonSerializer.WriteObject(fileStream, tmpList);
                }
                this.ApplicationState = IsSkyDriveRunning ? "Connected" : "Not connected";
            }
            catch (Exception)
            {
                this.ApplicationState = "Could not write your data to SkyDrive";
            }
        }

        private void NotifyError(string errorstring)
        {
            if (ErrorNotification != null)
            {
                ErrorNotification(errorstring);
            }
        }

        public IEnumerable<string> ListOfCategories
        {
            get
            {
                return (from item in this.ProductItems select item.Category).Distinct();
            }
        }

        public IEnumerable<string> ListOfUnitSizes
        {
            get
            {
                return (from item in this.ProductItems select item.UnitSize).Distinct();
            }
        }

        public IEnumerable<string> ListOfPreferredShops
        {
            get
            {
                return (from item in this.ProductItems select item.PreferredShop).Distinct();
            }
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

            IsDirty = true;

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

            IsDirty = true;

            DecreaseQuantityCommand.RaiseCanExecuteChanged();
            IncreaseQuantityCommand.RaiseCanExecuteChanged();
        }

        private bool CanDecreaseQuantity(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return pi.DefaultQuantity >= 1;
        }

        public RelayCommand<object> AddToShoppingListCommand { get; set; }

        private void AddToList(object o)
        {
            ShoppingItem item = o as ShoppingItem;
            if (o != null)
            {
                item.IsOnShoppingList = true;
                ShoppingList.Refresh();
                ShoppingList.MoveCurrentTo(item);
                IsDirty = true;
                RaisePropertyChanged("VisibleProducts");
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
                ShoppingList.MoveCurrentToNext();
                if (ShoppingList.IsCurrentAfterLast)
                {
                    ShoppingList.MoveCurrentTo(item);
                    ShoppingList.MoveCurrentToPrevious();
                }
                item.IsOnShoppingList = false;
                ShoppingList.Refresh();
                IsDirty = true;
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

        public RelayCommand<object> EditItemCommand { get; set; }

        private void StartEditing(object o)
        {
            ShoppingListElement item = o as ShoppingListElement;
            if (o != null)
            {
                item.IsEditing = true;
            }
        }

        private bool CanStartEditing(object o)
        {
            ShoppingListElement item = o as ShoppingListElement;
            if (o != null)
            {
                return item.IsSelected;
            }
            return false;
        }

        public RelayCommand<object> SaveItemCommand { get; set; }

        private void CloseEditing(object o)
        {
            ShoppingListElement item = o as ShoppingListElement;
            if (o != null)
            {
                item.IsEditing = false;
                ShoppingList.Refresh();
                IsDirty = true;
            }
        }

        private bool CanCloseEditing(object o)
        {
            ShoppingListElement item = o as ShoppingListElement;
            if (o != null)
            {
                return item.IsEditing;
            }
            return false;
        }

        public RelayCommand<object> SaveCommand { get; set; }

        private void SaveAll(object o)
        {
            this.SyncProductFile();
        }

        private bool CanSaveAll(object o)
        {
            return IsDirty;
        }

        public RelayCommand<object> DeleteItemCommand { get; set; }

        private void DeleteItem(object o)
        {
            ShoppingListElement item = o as ShoppingListElement;
            if( item == null ) return;

            if (item.IsEditing)
            {
                (item as ShoppingListElement).IsEditing = false;
                ShoppingList.MoveCurrentToNext();
                if (ShoppingList.IsCurrentAfterLast)
                {
                    ShoppingList.MoveCurrentTo(item);
                    ShoppingList.MoveCurrentToPrevious();
                }
                item.IsDeleted = true;
                this.ShoppingList.Refresh();
            }
            else
            {
                item.IsDeleted = true;
            }
            this.IsDirty = true;
            RaisePropertyChanged("VisibleProducts");
        }

        private bool CanDeleteItem(object o)
        {
            return null != o as ShoppingItem;
        }

        public override void Cleanup()
        {
            if( IsDirty )
            {
                this.SyncProductFile();
            }
            base.Cleanup();
        }

        public bool IsSkyDriveRunning
        {
            get
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    if (p.ProcessName.StartsWith("SkyDrive"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public bool DoesProductFileExists {
            get
            {
                return File.Exists(Productsfile);
            }
        }
    }
}