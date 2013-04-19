using System.ComponentModel;
using System.Runtime.Serialization;
using System;

namespace OneShoppingList.Model
{
    [DataContract]
    public class ProductItem : SyncItemBase
    {
        public ProductItem()
            : base()
        {
        }

        [DataMember]
        public string preferred_shop = "";
        public const string PreferredShopPropertyName = "PreferredShop";
        public string PreferredShop
        {
            get
            {
                return preferred_shop;
            }

            set
            {
                if (preferred_shop == value)
                {
                    return;
                }
                preferred_shop = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(PreferredShopPropertyName);
            }
        }

        [DataMember]
        public string category = "";
        public const string CategoryPropertyName = "Category";
        public string Category
        {
            get
            {
                return category;
            }

            set
            {
                if (category == value)
                {
                    return;
                }

                category = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(CategoryPropertyName);
            }
        }

        [DataMember]
        public string caption = "";
        public const string CaptionPropertyName = "Caption";
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption == value)
                {
                    return;
                }
                caption = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(CaptionPropertyName);
            }
        }

        [DataMember]
        public int default_quantity = 1;
        public const string DefaultQuantityPropertyName = "DefaultQuantity";
        public int DefaultQuantity
        {
            get
            {
                return default_quantity;
            }
            set
            {
                if (default_quantity == value)
                {
                    return;
                }
                default_quantity = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(DefaultQuantityPropertyName);
            }
        }

        [DataMember]
        public string unit_size = "";
        public const string UnitSizePropertyName = "UnitSize";
        public string UnitSize
        {
            get
            {
                return unit_size;
            }

            set
            {
                if (unit_size == value)
                {
                    return;
                }

                unit_size = value;
                base.TimeStamp = DateTime.Now;
                RaisePropertyChanged(UnitSizePropertyName);
            }
        }

        public override void Update(SyncItemBase source)
        {
            ProductItem pi = source as ProductItem;
            if( pi != null)
            {
                // we use the properties as we want the notifications to be performed.
                // the timestamp will be set last to the timestamp of the target
                this.PreferredShop = pi.PreferredShop;
                this.Category = pi.Category;
                this.Caption = pi.Caption;
                this.DefaultQuantity = pi.DefaultQuantity;
                this.UnitSize = pi.UnitSize;
            }
            base.Update(source);
        }

        public override SyncItemBase Clone()
        {
            ProductItem pi = new ProductItem();
            pi.Update(this);
            return pi;
        }
    }

}
