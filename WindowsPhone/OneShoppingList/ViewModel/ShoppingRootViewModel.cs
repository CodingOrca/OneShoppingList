using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OneShoppingList.ViewModel
{
    public class ShoppingRootViewModel : ObservableCollection<ItemsInCategoryViewModel>
    {
        private static readonly string Groups = "#ABCDEFGHIJKLMNOPQRSTUVWXYZ";


        public void Initialize(IEnumerable<ItemViewModel> list)
        {
            Dictionary<string, ItemsInCategoryViewModel> groups = new Dictionary<string, ItemsInCategoryViewModel>();

            foreach (char c in Groups)
            {
                ItemsInCategoryViewModel group = new ItemsInCategoryViewModel(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (ItemViewModel item in list)
            {
                groups[GetGroupKey(item)].Add(item);
            }
        }

        private string GetGroupKey(ItemViewModel item)
        {
            char key = char.ToUpper(item.ItemName[0]);

            if (key == 'Ö') key = 'O';
            else if (key == 'Ü') key = 'U';
            else if (key == 'Ä') key = 'A';
            else if (key < 'A' || key > 'Z')
            {
                key = '#';
            }
            return key.ToString();
        }

        public ShoppingRootViewModel()
        {
        }

    }
}