using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OneShoppingList.ViewModel
{
    public class ItemsInCategoryViewModel : ObservableCollection<ItemViewModel>
    {    
        public ItemsInCategoryViewModel(string category)
        {
            Key = category;
        }

        public string Key{get;set;}

        public bool HasItems { get { return Count > 0; } }
    }
}