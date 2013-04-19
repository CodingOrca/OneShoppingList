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
using LinqToVisualTree;
using OneShoppingList.ViewModel;

namespace OneShoppingList
{
    public partial class ShoppingItemContextMenu : ContextMenu
    {
        public event EventHandler OnRemove;
        public event EventHandler OnDelete;
        public event EventHandler OnEdit;
        public event EventHandler OnAddFavorite;
        public event EventHandler OnRemoveFavorite;
        
        private ViewModelLocator locator;
        
        public Visibility DeleteVisibility { get; set; }
        public Visibility RemoveVisibility { get; set; }

        public ShoppingItemContextMenu()
            : this(false)
        {
            locator = App.Current.Resources["Locator"] as ViewModelLocator;
        }

        public ShoppingItemContextMenu( bool ShowDeleteButton)
        {
            InitializeComponent();
            if (ShowDeleteButton)
            {
                DeleteVisibility = Visibility.Visible;
                RemoveVisibility = Visibility.Collapsed;
            }
            else
            {
                DeleteVisibility = Visibility.Collapsed;
                RemoveVisibility = Visibility.Visible;
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnRemove != null)
            {
                OnRemove(this, null);
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnDelete != null)
            {
                OnDelete(this, null);
            }
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnEdit != null)
            {
                OnEdit(this, null);
            }
        }

        private void addtoFavButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnAddFavorite != null)
            {
                OnAddFavorite(this, null);
            }
        }

        private void removeFavButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnRemoveFavorite != null)
            {
                OnRemoveFavorite(this, null);
            }
        }
    }
}
