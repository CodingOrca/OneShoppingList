using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ListUtils;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System;


namespace OneShoppingList.Model
{

    [DataContract]
    public class Shop : SyncItemBase
    {

        public override SyncItemBase Clone()
        {
            Shop newShop = new Shop();
            newShop.Update(this);
            return newShop;
        }

        public override void Update(SyncItemBase source)
        {
            Shop shop = source as Shop;
            if (shop != null)
            {
                this.Name = shop.Name;
                this.OrderedCategories.Clear();
                foreach (string s in shop.OrderedCategories)
                {
                    this.OrderedCategories.Add(s);
                }
            }
            base.Update(source);
        }

        private bool IsProductItemVisible(object o)
        {
            ShoppingItem pi = o as ShoppingItem;
            if (!pi.IsOnShoppingList) return false;
            if (pi.IsDeleted) return false;
            return true;
        }

        private ListViewModel<ShoppingItem> shoppingListViewModel;
        public ListViewModel<ShoppingItem> ShoppingListViewModel
        {
            get
            {
                if (shoppingListViewModel == null)
                {
                    shoppingListViewModel = new ListViewModel<ShoppingItem>();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            shoppingListViewModel.GroupProvider = new PropertyGroupDescription { PropertyName = "Category" };
                            ShoppingListViewModel.GroupsSortProvider = this.CategoryComparer;
                            shoppingListViewModel.Filter = IsProductItemVisible;
                            shoppingListViewModel.Source = DataLocator.Current.ProductItems;
                        }
                    );
                }
                return shoppingListViewModel;
            }
        }


        [DataMember]
        public string name = "";
        public const string NamePropertyName = "Name";
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name == value)
                {
                    return;
                }
                name = value;
                this.TimeStamp = DateTime.Now;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        public const string OrderedCategoriesPropertyName = "OrderedCategories";
        private ObservableCollection<string> orderedCategories = new ObservableCollection<string>();

        [DataMember]
        public ObservableCollection<string> OrderedCategories
        {
            get
            {
                return orderedCategories;
            }
            set
            {
                if (orderedCategories == value)
                {
                    return;
                }
                orderedCategories = value;
                RaisePropertyChanged(OrderedCategoriesPropertyName);
            }
        }

        public const string SelectedCategoryPropertyName = "SelectedCategory";
        private ShopCategoryAssignement selectedCategory = null;

        public ShopCategoryAssignement SelectedCategory
        {
            get
            {
                return selectedCategory;
            }

            set
            {
                if (selectedCategory == value)
                {
                    return;
                }
                selectedCategory = value;
                RaisePropertyChanged(SelectedCategoryPropertyName);
                this.CommandsCanExecuteChanged();
            }
        }

        private RelayCommand moveUpCommand; 
        public RelayCommand MoveUpCommand
        {
            get
            {
                if (this.moveUpCommand == null)
                {
                    moveUpCommand = new RelayCommand(this.MoveUp, this.CanMoveUp);
                }
                return moveUpCommand;
            }

        }

        private bool CanMoveUp()
        {
            if (this.SelectedCategory == null) return false;
            int index = this.OrderedCategories.IndexOf(this.SelectedCategory.CategoryName);
            return index > 0;
        }

        private void MoveUp()
        {
            ShopCategoryAssignement current = this.SelectedCategory;
            if (current != null)
            {
                int index = this.OrderedCategories.IndexOf(current.CategoryName);
                if (index > 0)
                {
                    this.OrderedCategories.RemoveAt(index);
                    this.OrderedCategories.Insert(index - 1, current.CategoryName);
                    current.RaisePropertyChanged(ShopCategoryAssignement.IsAssignedPropertyName);
                    this.SelectedCategory = current;
                    this.TimeStamp = DateTime.Now;
                    RaiseNotificationChanged(current.CategoryName);
                }
            }

            this.CommandsCanExecuteChanged();
        }

        private void RaiseNotificationChanged(string p)
        {
            foreach(ShoppingItem item in DataLocator.Current.ProductItems.Where(pi => pi.Category == p && !pi.IsDeleted))
            {
                item.RaisePropertyChanged(ShoppingItem.CategoryPropertyName);
            }
        }

        private void CommandsCanExecuteChanged()
        {
            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }

        private RelayCommand moveDownCommand;
        public RelayCommand MoveDownCommand 
        { 
            get
            {
                if (moveDownCommand == null)
                {
                    moveDownCommand = new RelayCommand(this.MoveDown, this.CanMoveDown);
                }
                return moveDownCommand;
            }
        }

        private void MoveDown()
        {
            ShopCategoryAssignement current = this.SelectedCategory;
            if (current != null)
            {
                int index = this.OrderedCategories.IndexOf(current.CategoryName);
                if (index >= 0 && index < this.OrderedCategories.Count - 1)
                {
                    this.OrderedCategories.RemoveAt(index);
                    this.OrderedCategories.Insert(index + 1, current.CategoryName);
                    current.RaisePropertyChanged(ShopCategoryAssignement.IsAssignedPropertyName);
                    this.SelectedCategory = current;
                    this.TimeStamp = DateTime.Now;
                    RaiseNotificationChanged(current.CategoryName);
                }
            }

            this.CommandsCanExecuteChanged();
        }

        private bool CanMoveDown()
        {
            if (this.SelectedCategory == null) return false;
            int index = this.OrderedCategories.IndexOf(this.SelectedCategory.CategoryName);
            return (index >= 0 && index < this.OrderedCategories.Count - 1);
        }

        private ObservableCollection<ShopCategoryAssignement> allCategories;
        public ObservableCollection<ShopCategoryAssignement> AllCategories
        {
            get
            {
                if (allCategories == null)
                {
                    allCategories = new ObservableCollection<ShopCategoryAssignement>(
                    DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted).Select(pi => pi.Category).Distinct().Select(cat => new ShopCategoryAssignement(cat, this))
                    );
                }
                return allCategories;
            }
        }

        private ListViewModel<ShopCategoryAssignement> categoryAssignements;
        public ListViewModel<ShopCategoryAssignement> CategoryAssignements 
        {
            get
            {
                if (categoryAssignements == null)
                {
                    categoryAssignements = new ListViewModel<ShopCategoryAssignement>();
                    categoryAssignements.Source = this.AllCategories;
                    categoryAssignements.ItemsSortProvider = new ShopCategoryAssignementComparer(this);
                    if (categoryAssignements.ItemsView.Count > 0)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            this.SelectedCategory = categoryAssignements.ItemsView[0];
                            this.CommandsCanExecuteChanged();
                        }
                        );
                    }
                }
                return categoryAssignements;
            }
        }

        private IComparer<string> categoryComparer;
        public IComparer<string> CategoryComparer 
        { 
            get 
            {
                if (categoryComparer == null)
                {
                    categoryComparer = new ShopCategoryComparer(this);
                }
                return categoryComparer;
            } 
        }

    }
}
