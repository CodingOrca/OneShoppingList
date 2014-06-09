using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneShoppingList.Model;
using System;
using System.Windows.Navigation;

namespace OneShoppingList_uTest.Tests
{
    [TestClass]
    public class TestClass1 : SilverlightTest
    {

        string[] categories = new string[]{
            "Gemüse",
            "Früchte",
            "Haushalt",
            "Wurstwahre",
            "Käsetheke",
            "Kühlregal",
            "Konserven",
            "Pasta und Mehl",
            "Süssigkeiten",
            "Getränke",
            "Drogerie"};

        [TestMethod]
        public void Save_1000_Items()
        {
            DataLocator.Current.ProductItems.Clear();
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                ShoppingItem item = new ShoppingItem();
                item.caption = categories[rnd.Next(11)] + "-ProductItem";
                item.PreferredShop = "Details";
                item.DefaultQuantity = 1;
                item.UnitSize = "Stück";
                item.Category = categories[rnd.Next(11)];
                DataLocator.Current.ProductItems.Add(item);
            }
            DataLocator.Current.SaveLocalData();
        }

        [TestMethod]
        public void Load_1000_Items()
        {
            DataLocator.Current.LoadLocalData();
        }
    }
}