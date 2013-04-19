using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using OneShoppingList.Model;
using System.Windows.Data;

namespace OneShoppingList.ViewModel
{
    public class ShopViewModel : INotifyPropertyChanged
    {
        MainViewModel mainViewModel;
        public ShopViewModel(MainViewModel mvm, string name)
        {
            mainViewModel = mvm;
            Name = name;
            this.itemsViewSource = new CollectionViewSource();
            this.itemsViewSource.Source = mvm.FlatItems;
            this.ItemsView.Filter = this.IsProductItemVisible;
        }

        private bool IsProductItemVisible(object o)
        {
            ProductItem pi = o as ProductItem;
            if (pi.IsInShoppingBasket) return false;
            if (!pi.IsOnShoppingList) return false;
            return true;// pi.PreferredShop == this.Name;
        }


        public string Name { get; set; }

        private CollectionViewSource itemsViewSource;
        public ICollectionView ItemsView
        {
            get
            {
                return itemsViewSource.View;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }
    }
}
