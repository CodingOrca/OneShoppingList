using System;
using System.Collections.Generic;

namespace OneShoppingList.Model
{

    public class ShoppingItem
    {
        public string Key { get; set; }
        public DateTime TimeStamp { get; set; }
        public string caption { get; set; }
        public string category { get; set; }
        public int default_quantity { get; set; }
        public string preferred_shop { get; set; }
        public string unit_size { get; set; }
        public bool is_favorite { get; set; }
        public bool is_in_basket { get; set; }
        public bool is_on_shopping_list { get; set; }
    }
}
