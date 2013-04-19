using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using OneShoppingList.Model;
using System.Collections.Generic;
using System.Linq;
using OneShoppingList.Resources;
using System;
using ListUtils;

namespace OneShoppingList.ViewModel
{
    public class AddProductItemViewModel : ViewModelBase
    {
        public AddProductItemViewModel()
        {
            Init();
        }

        public void Init()
        {
            ProductName = "";
            this.EditItem = null;
            this.PreferredShop = null; // must be the private member, not the property!

            this.UpdateProductItems();
            this.UpdateProductCategories();
            this.UpdateUnitSizes();
        }

        public AppResources Localized
        {
            get
            {
                return new LocalizedStrings().LocalizedResources;
            }
        }

        public const string ProductNamePropertyName = "ProductName";
        private string productName = "";
        public string ProductName
        {
            get
            {
                return productName;
            }

            set
            {
                if (productName == value)
                {
                    return;
                }
                productName = value;
                RaisePropertyChanged(ProductNamePropertyName);
            }
        }

        public const string KeyPropertyName = "Key";
        private Guid key = Guid.Empty;
        public Guid Key
        {
            get
            {
                return key;
            }

            set
            {
                if (key == value)
                {
                    return;
                }
                key = value;
                RaisePropertyChanged(KeyPropertyName);
            }
        }

        public const string ProductCategoryPropertyName = "ProductCategory";
        private string productCategory = "";
        public string ProductCategory
        {
            get
            {
                return productCategory;
            }

            set
            {
                if (productCategory == value)
                {
                    return;
                }
                productCategory = value;
                RaisePropertyChanged(ProductCategoryPropertyName);
            }
        }

        public const string PreferredShopPropertyName = "PreferredShop";
        private string preferredShop = null;
        public string PreferredShop
        {
            get
            {
                return preferredShop;
            }

            set
            {
                if (preferredShop == value)
                {
                    return;
                }
                preferredShop = value;
                RaisePropertyChanged(PreferredShopPropertyName);
            }
        }

        public const string QuantityPropertyName = "Quantity";
        private int quantity = 1;
        public int Quantity
        {
            get
            {
                return quantity;
            }

            set
            {
                if (quantity == value)
                {
                    return;
                }
                quantity = value;
                RaisePropertyChanged(QuantityPropertyName);
            }
        }

        public const string UnitSizePropertyName = "UnitSize";
        private string unitSize = "";
        public string UnitSize
        {
            get
            {
                return unitSize;
            }

            set
            {
                if (unitSize == value)
                {
                    return;
                }
                unitSize = value;
                RaisePropertyChanged(UnitSizePropertyName);
            }
        }

        public IEnumerable<string> Shops
        {
            get
            {
                return DataLocator.Current.Shops.Where(s => !s.IsDeleted).Select(s => s.Name);
            }
        }

        public const string ProductItemsPropertyName = "ProductItems";
        private List<ShoppingItem> productItems = null;
        public List<ShoppingItem> ProductItems
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

        private void UpdateProductItems()
        {
            this.ProductItems = DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted).ToList();
        }

        public const string ProductCategoriesPropertyName = "ProductCategories";
        private List<string> productCategories = null;
        public List<string> ProductCategories
        {
            get
            {
                return productCategories;
            }
            set
            {
                if (productCategories == value)
                {
                    return;
                }

                productCategories = value;
                RaisePropertyChanged(ProductCategoriesPropertyName);
            }
        }

        private void UpdateProductCategories()
        {
            this.ProductCategories = this.ProductItems.Select(pi => pi.Category).Distinct().ToList();
        }

        public const string UnitSizesPropertyName = "UnitSizes";
        private List<string> unitSizes = null;
        public List<string> UnitSizes
        {
            get
            {
                return unitSizes;
            }

            set
            {
                if (unitSizes == value)
                {
                    return;
                }

                unitSizes = value;
                RaisePropertyChanged(UnitSizesPropertyName);
            }
        }

        private void UpdateUnitSizes()
        {
            this.UnitSizes = DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted).Select(pi => pi.UnitSize).Distinct().ToList();
        }

        public void Save()
        {
            ShoppingItem p = null;

            if (EditItem != null)
            {
                p = EditItem;
            }
            else if (DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted).Count(pi => (pi.Key == this.Key && pi.Caption == this.ProductName)) > 0)
            {
                p = DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted).Where(pi => pi.Key == this.Key).First();
            }
            else
            {
                p = new ShoppingItem();
                DataLocator.Current.ProductItems.Add(p);
            }
            
            p.Caption = this.ProductName;
            
            if (String.IsNullOrWhiteSpace(this.ProductCategory))
            {
                this.ProductCategory = AppResources.defaultCategory;
            }
            p.Category = this.ProductCategory;
            p.PreferredShop = this.PreferredShop;

            if (String.IsNullOrWhiteSpace(this.UnitSize))
            {
                this.UnitSize = AppResources.defaultUnit;
            }

            p.UnitSize = this.UnitSize;
            p.DefaultQuantity = this.Quantity;
            p.IsOnShoppingList = true;
            p.IsDeleted = false;

            this.UpdateProductItems();
            this.UpdateProductCategories();
            this.UpdateUnitSizes();
        }

        public void DeleteEditItem()
        {
            if (EditItem != null)
            {
                EditItem.IsDeleted = true;
            }
        }

        public const string EditItemPropertyName = "EditItem";
        private ShoppingItem editItem = null;
        public ShoppingItem EditItem
        {
            get
            {
                return editItem;
            }

            set
            {
                if (editItem == value)
                {
                    return;
                }
                editItem = value;
                SelectedProductItem = editItem;
                RaisePropertyChanged(EditItemPropertyName);
            }
        }

        public const string SelectedProductItemPropertyName = "SelectedProductItem";
        private ShoppingItem selectedProductItem = null;
        public ShoppingItem SelectedProductItem
        {
            get
            {
                return selectedProductItem;
            }

            set
            {
                if (selectedProductItem == value)
                {
                    return;
                }
                selectedProductItem = value;
                RaisePropertyChanged(SelectedProductItemPropertyName);
                if (selectedProductItem != null)
                {
                    this.ProductName = selectedProductItem.Caption;
                    this.Key = selectedProductItem.Key;
                    this.ProductCategory = selectedProductItem.Category;
                    this.Quantity = selectedProductItem.DefaultQuantity;
                    this.UnitSize = selectedProductItem.UnitSize;
                    this.PreferredShop = selectedProductItem.PreferredShop;
                }
            }
        }

        public void SetEditItem(Guid key)
        {
            ShoppingItem p = DataLocator.Current.ProductItems.Where(pi => !pi.IsDeleted && pi.Key == key).FirstOrDefault();
            if (p != null)
            {
                EditItem = p;
            }
            else
            {
                EditItem = null;
            }
        }

        internal void Clear()
        {
            SelectedProductItem = null;
            this.ProductName = "";
        }
    }
}