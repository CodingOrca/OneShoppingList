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

namespace OneShoppingList.Model
{

    public class ShopCategoryAssignement : INotifyPropertyChanged
    {
        public ShopCategoryAssignement(string categoryName, Shop shop)
        {
            this.CategoryName = categoryName;
            this.Shop = shop;
            this.isAssigned = Shop.OrderedCategories.Contains(categoryName);
        }

        public Shop Shop { get; private set; }

        public const string CategoryNamePropertyName = "CategoryName";
        private string categoryName = null;
        public string CategoryName 
        {
            get
            {
                return categoryName;
            }
            set
            {
                if (value == categoryName)
                {
                    return;
                }
                categoryName = value;
                RaisePropertyChanged(CategoryNamePropertyName);
            }
        }

        public const string IsAssignedPropertyName = "IsAssigned";
        private bool isAssigned = false;
        
        public bool IsAssigned
        {
            get
            {
                return isAssigned;
            }

            set
            {
                if (isAssigned == value)
                {
                    return;
                }
                isAssigned = value;
                if (isAssigned)
                {
                    this.Shop.OrderedCategories.Add(this.CategoryName);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            this.Shop.SelectedCategory = this;
                        }
                    );
                }
                else
                {
                    this.Shop.OrderedCategories.Remove(this.CategoryName);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        this.Shop.SelectedCategory = this;
                    }
                    );
                }
                this.Shop.TimeStamp = DateTime.Now;
                RaisePropertyChanged(IsAssignedPropertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }
    }
}
