using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Resources;
using OneShoppingList.Resources;

namespace OneShoppingList.Model
{
    public class DataLocator
    {
        private static DataLocator current;
        private bool isValidData = false;

        public static DataLocator Current 
        {
            get
            {
                if (current == null)
                {
                    current = new DataLocator();
                }
                return current;
            }
        }

        public ObservableCollection<ShoppingItem> ProductItems { get; private set; }

        public ObservableCollection<Shop> Shops { get; private set; }

        private DataLocator()
        {
            ProductItems = new ObservableCollection<ShoppingItem>();
            Shops = new ObservableCollection<Shop>();
        }

        public void LoadLocalData()
        {
            ProductItems.Clear();
            Shops.Clear();
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists("OneShoppingList.json"))
                {
                    using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingList.json", FileMode.Open))
                    {

                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                        object o = ser.ReadObject(stream);
                        List<ShoppingItem> readItems = o as List<ShoppingItem>;
                        if (readItems != null)
                        {
                            for (int i = 0; i < readItems.Count; i++)
                            {
                                ProductItems.Add(readItems[i]);
                            }
                        }
                    }
                }
                if (isf.FileExists("OneShoppingStores.json"))
                {
                    using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingStores.json", FileMode.Open))
                    {

                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                        object o = ser.ReadObject(stream);
                        List<Shop> readItems = o as List<Shop>;
                        if (readItems != null)
                        {
                            for (int i = 0; i < readItems.Count; i++)
                            {
                                Shops.Add(readItems[i]);
                            }
                        }

                    }
                }
            }
            isValidData = true;
        }

        public void LoadDefaultData( string itemsfilename, string shopsfilename)
        {
            ProductItems.Clear();
            DateTime now = DateTime.Now;
            StreamResourceInfo info = Application.GetResourceStream(new Uri(itemsfilename, UriKind.Relative));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
            object o = ser.ReadObject(info.Stream);
            List<ShoppingItem> readItems = o as List<ShoppingItem>;
            if (readItems != null)
            {
                for (int i = 0; i < readItems.Count; i++)
                {
                    readItems[i].TimeStamp = now;
                    ProductItems.Add(readItems[i]);
                }
            }

            Shops.Clear();
            info = Application.GetResourceStream(new Uri(shopsfilename, UriKind.Relative));
            ser = new DataContractJsonSerializer(typeof(List<Shop>));
            o = ser.ReadObject(info.Stream);
            List<Shop> shopItems = o as List<Shop>;
            if (shopItems != null)
            {
                for (int i = 0; i < shopItems.Count; i++)
                {
                    shopItems[i].TimeStamp = now;
                    Shops.Add(shopItems[i]);
                }
            }
        }

        public void SaveLocalData()
        {
            // TODO Error handling?
            if (!isValidData) return;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingList.json", FileMode.Create))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                    ser.WriteObject(stream, new List<ShoppingItem>(this.ProductItems));
                }
                using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingStores.json", FileMode.Create))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                    ser.WriteObject(stream, new List<Shop>(this.Shops));
                }
            }
        }
    }
}
