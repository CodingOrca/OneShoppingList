using System.Collections.Generic;
using System;

namespace OneShoppingList.Model
{
    public class ShopCategoryComparer: IComparer<string>
    {
        private Shop shop;
        public ShopCategoryComparer(Shop s)
        {
            shop = s;
        }

        public int Compare(string x, string y)
        {
            if (shop.OrderedCategories == null) return String.Compare(x, y);
            int indX = shop.OrderedCategories.IndexOf(x);
            int indY = shop.OrderedCategories.IndexOf(y);
            if (indX < 0) indX = Int32.MaxValue;
            if (indY < 0) indY = Int32.MaxValue;
            if (indX < indY) return -1;
            else if (indX == indY) return String.Compare(x, y);
            else return 1;
        }
    }

    public class ShopCategoryAssignementComparer : IComparer<ShopCategoryAssignement>
    {
        private Shop shop;
        public ShopCategoryAssignementComparer(Shop s)
        {
            shop = s;
        }

        public int Compare(ShopCategoryAssignement x, ShopCategoryAssignement y)
        {
            if (shop.OrderedCategories == null) return String.Compare(x.CategoryName, y.CategoryName);
            int indX = shop.OrderedCategories.IndexOf(x.CategoryName);
            int indY = shop.OrderedCategories.IndexOf(y.CategoryName);
            if (indX < 0) indX = Int32.MaxValue;
            if (indY < 0) indY = Int32.MaxValue;
            if (indX < indY) return -1;
            else if (indX == indY) return String.Compare(x.CategoryName, y.CategoryName);
            else return 1;
        }
    }
}
