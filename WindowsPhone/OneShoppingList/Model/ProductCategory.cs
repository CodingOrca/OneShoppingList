using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace OneShoppingList.Model
{
    public class ProductCategory: ObservableCollection<ProductItem>, INotifyPropertyChanged
    {
        public const string CaptionPropertyName = "Caption";
        private string caption = "";
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption == value)
                {
                    return;
                }
                caption = value;
                RaisePropertyChanged(CaptionPropertyName);
            }
        }

        public bool HasProductItems { get { return base.Count > 0; } }
        public bool HasShoppingItems
        {
            get
            {
                foreach (ProductItem item in this)
                {
                    if (item.IsOnShoppingList == true) return true;
                }
                return false;
            }
        }

        public bool HasOpenShoppingItems
        {
            get
            {
                foreach (ProductItem item in this)
                {
                    if (item.IsOnShoppingList == true && !item.IsInShoppingBasket) return true;
                }
                return false;
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
