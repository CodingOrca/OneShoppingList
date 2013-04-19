using System.Runtime.Serialization;
using System;

namespace OneShoppingList.Model
{
    [DataContract]
    public class ShoppingItem : ProductItem
    {
        public ShoppingItem()
            : base()
        {
        }

        [DataMember]
        public bool is_favorite = false;
        public const string IsFavoritePropertyName = "IsFavorite";
        public bool IsFavorite
        {
            get
            {
                return is_favorite;
            }

            set
            {
                if (is_favorite == value)
                {
                    return;
                }

                is_favorite = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(IsFavoritePropertyName);
            }
        }

        [DataMember]
        public bool is_on_shopping_list = false;
        public const string IsOnShoppingListPropertyName = "IsOnShoppingList";
        public bool IsOnShoppingList
        {
            get
            {
                return is_on_shopping_list;
            }
            set
            {
                if (is_on_shopping_list == value)
                {
                    return;
                }
                is_on_shopping_list = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(IsOnShoppingListPropertyName);
            }
        }

        public override void Update(SyncItemBase source)
        {
            ShoppingItem si = source as ShoppingItem;
            if (si != null)
            {
                this.IsFavorite = si.IsFavorite;
                this.IsOnShoppingList = si.IsOnShoppingList;
            }
            base.Update(source);
        }

        public override SyncItemBase Clone()
        {
            ShoppingItem si = new ShoppingItem();
            si.Update(this);
            return si;
        }

    }

}
