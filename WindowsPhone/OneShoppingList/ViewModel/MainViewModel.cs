using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using GalaSoft.MvvmLight;
using OneShoppingList.ViewModel;
using OneShoppingList.Model;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using System.Xml;
using ListUtils;
using Microsoft.Phone.Shell;
using System.Windows.Resources;
using OneShoppingList.Resources;


namespace OneShoppingList
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.ToggleInShoppingBasket = new RelayCommand<object>(this.ToggleInBasket);
            this.IncreaseQuantityCommand = new RelayCommand<object>(this.IncreaseQuantity, this.CanIncreaseQuantity);
            this.DecreaseQuantityCommand = new RelayCommand<object>(this.DecreaseQuantity, this.CanDecreaseQuantity);
            this.RemoveFromShoppingListCommand = new RelayCommand<object>(this.RemoveFromShoppingList, this.CanRemoveFromShoppingList);
            this.DeleteProductItemCommand = new RelayCommand<object>(this.DeleteProductItem, this.CanDeleteProductItem);
            this.AddToFavoritesCommand = new RelayCommand<object>(this.AddToFavorites, this.CanAddToFavorites);
            this.RemoveFavoriteCommand = new RelayCommand<object>(this.RemoveFavorite, this.CanRemoveFavorite);
            this.AddShopCommand = new RelayCommand<object>(this.AddShop);
            this.DeleteShopCommand = new RelayCommand<object>(this.DeleteShop, this.CanDeleteShop);
            this.SyncCommand = new RelayCommand(this.Sync, this.CanSync);

            if (!base.IsInDesignMode)
            {
                LoadData();
                this.SyncHandler = new SyncHandler();
                this.SyncHandler.PropertyChanged += new PropertyChangedEventHandler(SkyDriveHandler_PropertyChanged);
            }

            ListViewModel<Shop> shopsViewModel = new ListViewModel<Shop>();
            shopsViewModel.Filter = IsShopVisible;
            shopsViewModel.Source = DataLocator.Current.Shops;
            this.Shops = shopsViewModel.ItemsView;
            if (this.Shops.Count != 0)
                this.CurrentShop = Shops[0];

            ListViewModel<ShoppingItem> listViewModel = new ListViewModel<ShoppingItem>();
            listViewModel.Filter = IsOnShoppingList;
            listViewModel.Source = DataLocator.Current.ProductItems;
            this.UnsortedShoppingList = listViewModel.ItemsView;
        }

        public ObservableCollection<ShoppingItem> UnsortedShoppingList { get; set; }

        void SkyDriveHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.SyncCommand.RaiseCanExecuteChanged();
        }

        public void ResetSyncHandler()
        {
            SyncHandler.Reset();
        }

        public AppResources Localized
        {
            get
            {
                return new LocalizedStrings().LocalizedResources;
            }
        }

        private bool IsShopVisible(object o)
        {
            Shop s = o as Shop;
            return !s.IsDeleted;
        }

        private bool IsOnShoppingList(object o)
        {
            ShoppingItem s = o as ShoppingItem;
            return s.IsOnShoppingList;
        }

        private bool IsProductItemFavorit(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi.IsDeleted) return false;
            return pi.IsFavorite;
        }

        private bool IsProductItemVisible(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            return !pi.IsDeleted;
        }

        private ListViewModel<ShoppingItem> favoritesViewModel;
        public ListViewModel<ShoppingItem> FavoritesViewModel
        {
            get
            {
                if (favoritesViewModel == null)
                {
                    favoritesViewModel = new ListViewModel<ShoppingItem>();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            favoritesViewModel.GroupProvider = new PropertyGroupDescription { PropertyName = "Category" };
                            favoritesViewModel.Filter = IsProductItemFavorit;
                            favoritesViewModel.ItemsSortProvider = new ShoppingCaptionComparer();
                            favoritesViewModel.Source = DataLocator.Current.ProductItems;
                        });
                }
                return favoritesViewModel;
            }
        }

        private ListViewModel<ShoppingItem> allItemsViewModel;
        public ListViewModel<ShoppingItem> AllItemsViewModel
        {
            get
            {
                if(allItemsViewModel == null )
                {
                    allItemsViewModel = new ListViewModel<ShoppingItem>();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            allItemsViewModel.GroupProvider = new PropertyGroupDescription { PropertyName = "Category" };
                            allItemsViewModel.Filter = IsProductItemVisible;
                            allItemsViewModel.ItemsSortProvider = new ShoppingCaptionComparer();
                            allItemsViewModel.Source = DataLocator.Current.ProductItems;
                        });
                }
                return allItemsViewModel;
            }
        }

        public class ShoppingItemTimeStampComparer : IComparer<ShoppingItem>
        {
            public int Compare(ShoppingItem x, ShoppingItem y)
            {
                return y.TimeStamp.CompareTo(x.TimeStamp);
            }
        }

        public class ShoppingCaptionComparer : IComparer<ShoppingItem>
        {
            public int Compare(ShoppingItem x, ShoppingItem y)
            {
                return x.Caption.CompareTo(y.Caption);
            }
        }

        public List<ShoppingItem> RecentList
        {
            get
            {
                var ordered = from pi in DataLocator.Current.ProductItems 
                              where !pi.IsDeleted && !pi.IsOnShoppingList
                              orderby pi.TimeStamp descending
                              select pi;
                return ordered.Take(25).ToList();
            }
        }

        public SyncHandler SyncHandler { get; private set; }
        private Shop currentShop = null;
        public Shop CurrentShop 
        { 
            get
            {
                return currentShop;
            }
            set
            {
                if (value == currentShop)
                {
                    return;
                }
                currentShop = value;
                RaisePropertyChanged("CurrentShop");
            }
        }

        public ObservableCollection<Shop> Shops { get; set; }

        public RelayCommand<object> ToggleInShoppingBasket { get; set; }

        private void ToggleInBasket(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            if (pi.IsOnShoppingList)
            {
                pi.IsOnShoppingList = false;
            }
            else
            {
                pi.IsOnShoppingList = true;
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

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public RelayCommand<object> RemoveFromShoppingListCommand { get; set; }

        private void RemoveFromShoppingList(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            if (pi.IsOnShoppingList) pi.IsOnShoppingList = false;
            RemoveFromShoppingListCommand.RaiseCanExecuteChanged();
        }

        private bool CanRemoveFromShoppingList(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return pi.IsOnShoppingList;
        }

        public RelayCommand<object> DeleteProductItemCommand { get; set; }

        private void DeleteProductItem(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi != null)
            {
                pi.IsFavorite = false;
                pi.IsOnShoppingList = false;
                pi.IsDeleted = true;
            }
        }

        private bool CanDeleteProductItem(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return DataLocator.Current.ProductItems.Contains(pi);
        }

        public RelayCommand<object> AddToFavoritesCommand { get; set; }

        private void AddToFavorites(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            pi.IsFavorite = true;
            AddToFavoritesCommand.RaiseCanExecuteChanged();
            RemoveFavoriteCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddToFavorites(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return !pi.IsFavorite;
        }

        public RelayCommand<object> RemoveFavoriteCommand { get; set; }

        private void RemoveFavorite(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return;
            pi.IsFavorite = false;
            AddToFavoritesCommand.RaiseCanExecuteChanged();
            RemoveFavoriteCommand.RaiseCanExecuteChanged();
        }

        private bool CanRemoveFavorite(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (pi == null) return false;
            return pi.IsFavorite;
        }

        public RelayCommand<object> AddShopCommand { get; set; }

        private void AddShop(object o)
        {
            Shop shop = null;
            if (this.CurrentShop != null)
            {
                shop = this.CurrentShop.Clone() as Shop;
                shop.Key = Guid.NewGuid();
            }
            else
            {
                shop = new Shop();
            }
            shop.Name = o as string;
            DataLocator.Current.Shops.Add(shop);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.CurrentShop = shop;
                }
            );
        }

        public RelayCommand<object> DeleteShopCommand { get; set; }

        private void DeleteShop(object o)
        {
            Shop shop = o as Shop;
            if (shop == null || !this.Shops.Contains(shop) ) return;
            shop.IsDeleted = true;
        }

        private bool CanDeleteShop(object o)
        {
            Shop shop = o as Shop;
            return shop != null && this.Shops.Contains(shop);
        }

        public RelayCommand SyncCommand { get; private set; }

        private void Sync()
        {
            this.SyncHandler.SyncAsync();
        }

        private bool CanSync()
        {
            return !this.SyncHandler.IsRunning;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            DataLocator.Current.LoadLocalData();
            this.IsDataLoaded = true;
        }

        internal void SaveData()
        {
            DataLocator.Current.SaveLocalData();
            ShellTile.ActiveTiles.First().Update(new StandardTileData() {
                Count = DataLocator.Current.ProductItems.Count(pi=>pi.IsOnShoppingList && !pi.IsDeleted)
            });
        }

        internal void RenameShop(string oldName, string newName)
        {
            Shop shop = this.Shops.Where(s => s.Name == oldName).SingleOrDefault();
            if (shop != null)
            {
                shop.Name = newName;
            }
            foreach (ShoppingItem pi in DataLocator.Current.ProductItems.Where(p => p.PreferredShop == oldName))
            {
                pi.PreferredShop = newName;
            }
        }

        internal bool IsValidShopName(string p)
        {
            return !String.IsNullOrWhiteSpace(p) && this.Shops.Count(s => s.Name == p ) == 0;
        }

        internal void ClearShoppingList()
        {
            foreach (ShoppingItem pi in DataLocator.Current.ProductItems)
            {
                pi.IsOnShoppingList = false;
            }
        }

        internal bool IsValidCategoryName(string oldName, string newName)
        {
            return oldName != newName && !String.IsNullOrWhiteSpace(newName);
        }

        internal bool CategoryExists(string newName)
        {
            return DataLocator.Current.ProductItems.Count(pi => pi.Category == newName && !pi.IsDeleted) > 0;
        }

        internal void RenameCategory(string oldName, string newName)
        {
            foreach (ShoppingItem pi in DataLocator.Current.ProductItems.Where(p => p.Category == oldName))
            {
                pi.Category = newName;
            }
            foreach (var shop in this.Shops)
            {
                int i = shop.OrderedCategories.IndexOf(oldName);
                if (i >= 0)
                {
                    shop.OrderedCategories.RemoveAt(i);
                    int j = shop.OrderedCategories.IndexOf(newName);
                    if (j < 0)
                    {
                        shop.OrderedCategories.Insert(i, newName);
                    }
                    // shop.RaisePropertyChanged(Shop.OrderedCategoriesPropertyName);
                }
                ShopCategoryAssignement oldCatAssignement = shop.AllCategories.Where(c => c.CategoryName == oldName).FirstOrDefault();
                if (oldCatAssignement != null)
                {
                    ShopCategoryAssignement newCatAssignement = shop.AllCategories.Where(c => c.CategoryName == newName).FirstOrDefault();
                    if (newCatAssignement != null)
                    {
                        shop.AllCategories.Remove(oldCatAssignement);
                    }
                    else
                    {
                        oldCatAssignement.CategoryName = newName;
                    }
                }
                shop.TimeStamp = DateTime.Now;
            }
        }
    }
}