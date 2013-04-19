using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;

namespace OneShoppingList
{
    public class ItemViewModel : ViewModelBase
    {
        private string itemName;
        public string ItemName
        {
            get
            {
                return itemName;
            }
            set
            {
                if (value != itemName)
                {
                    itemName = value;
                    RaisePropertyChanged("ItemName");
                }
            }
        }

        private bool isBuyed;
        public bool IsBuyed
        {
            get
            {
                return isBuyed;
            }
            set
            {
                if (value != isBuyed)
                {
                    isBuyed = value;
                    RaisePropertyChanged("IsBuyed");
                }
            }
        }

        private string category;
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                if (value != category)
                {
                    category = value;
                    RaisePropertyChanged("Category");
                }
            }
        }
    }
}