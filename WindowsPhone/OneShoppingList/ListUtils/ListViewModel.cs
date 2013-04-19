using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Specialized;
using System.Globalization;
using System.Collections.Generic;

namespace ListUtils
{
    public class ListViewModel<ItemType>: INotifyPropertyChanged
        where ItemType : class
    {
        public ListViewModel()
            : base()
        {
            GroupsView = new ObservableCollection<GroupViewModel<ItemType>>();
            ItemsView = new ObservableCollection<ItemType>();
            //ItemsSortProvider = new DefaultItemComparer(this);
            GroupsSortProvider = new DefaultGroupsComparer();
        }

        public ObservableCollection<GroupViewModel<ItemType>> GroupsView { get; set; }

        public ObservableCollection<ItemType> ItemsView { get; set; }

        public const string GroupProviderPropertyName = "GroupProvider";
        private GroupDescription groupProvider = null;
        public GroupDescription GroupProvider
        {
            get
            {
                return groupProvider;
            }

            set
            {
                if (groupProvider == value)
                {
                    return;
                }
                groupProvider = value;
                if (Source != null)
                {
                    RecreateGroupsView();
                }
                RaisePropertyChanged(GroupProviderPropertyName);
            }
        }

        public const string ItemsSortProviderPropertyName = "ItemsSortProvider";
        private IComparer<ItemType> itemsSortProvider = null;
        public IComparer<ItemType> ItemsSortProvider
        {
            get
            {
                return itemsSortProvider;
            }

            set
            {
                if (itemsSortProvider == value)
                {
                    return;
                }
                itemsSortProvider = value;
                RecreateViewModels();
                RaisePropertyChanged(ItemsSortProviderPropertyName);
            }
        }

        public const string GroupsSortProviderPropertyName = "GroupsSortProvider";
        private IComparer<string> groupsSortProvider = null;
        public IComparer<string> GroupsSortProvider
        {
            get
            {
                return groupsSortProvider;
            }

            set
            {
                if (groupsSortProvider == value)
                {
                    return;
                }
                groupsSortProvider = value;
                RecreateViewModels();
                RaisePropertyChanged(GroupsSortProviderPropertyName);
            }
        }

        public const string FilterPropertyName = "Filter";
        private Predicate<ItemType> filter = null;
        public Predicate<ItemType> Filter
        {
            get
            {
                return filter;
            }

            set
            {
                if (filter == value)
                {
                    return;
                }
                filter = value;
                if (Source != null)
                {
                    RecreateViewModels();
                }
                RaisePropertyChanged(FilterPropertyName);
            }
        }

        public const string SourcePropertyName = "Source";
        private ObservableCollection<ItemType> source = null;
        public ObservableCollection<ItemType> Source
        {
            get
            {
                return source;
            }

            set
            {
                if (source == value)
                {
                    return;
                }
                UnregisterNotifications();
                source = value;
                RecreateViewModels();
                RegisterNotifications();
                RaisePropertyChanged(SourcePropertyName);
            }
        }

        private void RegisterNotifications()
        {
            ItemsView.CollectionChanged += ItemsView_CollectionChanged;
            if (Source != null)
            {
                Source.CollectionChanged += source_CollectionChanged;
                foreach (ItemType i in Source)
                {
                    INotifyPropertyChanged notifyingObject = i as INotifyPropertyChanged;
                    if (notifyingObject != null)
                    {
                        notifyingObject.PropertyChanged += notifyingObject_PropertyChanged;
                    }
                }
            }
        }

        private void UnregisterNotifications()
        {
            ItemsView.CollectionChanged -= ItemsView_CollectionChanged;
            if (Source != null)
            {
                Source.CollectionChanged -= source_CollectionChanged;
                foreach (ItemType i in Source)
                {
                    INotifyPropertyChanged notifyingObject = i as INotifyPropertyChanged;
                    if (notifyingObject != null)
                    {
                        notifyingObject.PropertyChanged += notifyingObject_PropertyChanged;
                    }
                }
            }
        }

        public void RecreateViewModels()
        {
            RecreateItemsView();
            RecreateGroupsView();
        }

        private void RecreateItemsView()
        {
            ItemsView.Clear();
            if (Source == null) return;
            foreach(ItemType item in Source)
            {
                if (Filter == null || Filter(item))
                {
                    AddSortedItemTo(ItemsView, item);
                }
            }
        }

        private void AddSortedItemTo(ObservableCollection<ItemType> collection, ItemType item)
        {
            int index = ComputeItemInsertionIndex(collection, item);
            collection.Insert(index, item);
        }

        private int ComputeItemInsertionIndex(ObservableCollection<ItemType> collection, ItemType item)
        {
            int index = 0;
            while (index < collection.Count && (item == collection[index] || (ItemsSortProvider != null && ItemsSortProvider.Compare(collection[index], item) < 0)))
            {
                index++;
            }
            return index;
        }

        /// <summary>
        /// Clears every Group of the GrupsView (not the GroupsView itself!) and poplates them again from the ItemsView.
        /// </summary>
        private void RecreateGroupsView()
        {
            foreach (GroupViewModel<ItemType> g in this.GroupsView)
            {
                g.Clear();
            }
            this.GroupsView.Clear();

            if (GroupProvider == null) return;

            var groups = ItemsView.GroupBy(i => GroupProvider.GroupNameFromItem(i, 0, CultureInfo.CurrentUICulture));
            foreach (var group in groups)
            {
                GroupViewModel<ItemType> gvm = new GroupViewModel<ItemType>() { Key = group.Key.ToString()};
                AddSortedToGroupsView(gvm);
                foreach (ItemType item in group)
                {
                    AddSortedItemTo(gvm, item);
                }
            }
        }

        private void AddSortedToGroupsView(GroupViewModel<ItemType> gvm)
        {
            int index = ComputeGroupInsertionIndex(gvm);
            this.GroupsView.Insert(index, gvm);
        }

        private int ComputeGroupInsertionIndex(GroupViewModel<ItemType> gvm)
        {
            int index = 0;
            while (index < this.GroupsView.Count && (gvm == GroupsView[index] || GroupsSortProvider.Compare(GroupsView[index].Key, gvm.Key) < 0) )
            {
                index++;
            }
            return index;
        }

        void notifyingObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemType item = sender as ItemType;
            if (Filter == null || Filter(item))
            {
                if (!ItemsView.Contains(item))
                {
                    AddSortedItemTo(ItemsView, item);
                }
                else if (this.ItemsSortProvider != null && ItemsView.IndexOf(item) + 1 != ComputeItemInsertionIndex(ItemsView, item))
                {
                    ItemsView.Remove(item);
                    AddSortedItemTo(ItemsView, item);
                }
                else if (GroupProvider != null)
                {
                    string targetGroup = GroupProvider.GroupNameFromItem(item, 0, CultureInfo.CurrentUICulture).ToString();
                    List<GroupViewModel<ItemType>> list = this.GroupsView.Where(g => g.Contains(item)).ToList();
                    GroupViewModel<ItemType> currentGroup = list.SingleOrDefault();
                    if (targetGroup != currentGroup.Key)
                    {
                        // remove and insert the target at the same position, 
                        // the handler of the CollectionChanged event will take care to put the newly inserted target in the proper group.
                        int index = ItemsView.IndexOf(item);
                        ItemsView.Remove(item);
                        ItemsView.Insert(index, item);
                    }
                    else
                    {
                        // verify if the current group is at the proper position:
                        if (this.GroupsSortProvider != null && GroupsView.IndexOf(currentGroup) + 1 != ComputeGroupInsertionIndex(currentGroup))
                        {
                            // this two line are not working, they scrample the LongListSelector!
                            // this.GroupsView.Remove(currentGroup);
                            // AddSortedToGroupsView(currentGroup);
                            // so we remove all items and add the again:
                            List<ItemType> tmpList = new List<ItemType>(currentGroup);
                            foreach (ItemType i in tmpList)
                            {
                                ItemsView.Remove(i);
                            }
                            foreach (ItemType i in tmpList)
                            {
                                AddSortedItemTo(ItemsView, i);
                            }
                        }
                    }
                }
            }
            else
            {
                if (ItemsView.Contains(item))
                {
                    ItemsView.Remove(item);
                }
            }
        }

        void ItemsView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.GroupProvider == null) return;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object o in e.NewItems)
                {
                    string key = this.GroupProvider.GroupNameFromItem(o, 0, CultureInfo.CurrentUICulture).ToString();
                    var gvm = this.GroupsView.Where(g => g.Key == key).SingleOrDefault();
                    if (gvm == null)
                    {
                        gvm = new GroupViewModel<ItemType>() { Key = key };
                        AddSortedToGroupsView(gvm);
                    }
                    AddSortedItemTo(gvm, o as ItemType);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object o in e.OldItems)
                {
                    var gvm = this.GroupsView.Where(g => g.Contains(o as ItemType)).SingleOrDefault();
                    gvm.Remove(o as ItemType);
                    if (!gvm.HasItems)
                    {
                        this.GroupsView.Remove(gvm);
                    }
                }
            }
            else
            {
                RecreateGroupsView();
            }
        }

        void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object o in e.NewItems)
                {
                    if (this.Filter == null || this.Filter(o as ItemType))
                    {
                        AddSortedItemTo(ItemsView, o as ItemType);
                    }
                    INotifyPropertyChanged notificable = o as INotifyPropertyChanged;
                    if (notificable != null)
                    {
                        notificable.PropertyChanged += notifyingObject_PropertyChanged;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object o in e.OldItems)
                {
                    if( ItemsView.Contains(o as ItemType) )
                    {
                        ItemsView.Remove(o as ItemType);
                    }
                    INotifyPropertyChanged notificable = o as INotifyPropertyChanged;
                    if (notificable != null)
                    {
                        notificable.PropertyChanged -= notifyingObject_PropertyChanged;
                    }
                }
            }
            else
            {
                RecreateItemsView();
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        private class DefaultItemComparer : IComparer<ItemType>
        {
            ListViewModel<ItemType> listViewModel;

            public DefaultItemComparer(ListViewModel<ItemType> lvm)
            {
                listViewModel = lvm;
            }

            public int Compare(ItemType x, ItemType y)
            {
                if (listViewModel.Source == null) return 0;
                for (int i = 0; i < listViewModel.Source.Count; i++)
                {
                    if (listViewModel.Source[i] == x)
                    {
                        if (listViewModel.Source[i] == y)
                        {
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (listViewModel.Source[i] == y)
                    {
                        return 1;
                    }
                }
                return 0;
                //int indexX = listViewModel.Source.IndexOf(x);
                //int indexY = listViewModel.Source.IndexOf(y);
                //if (indexX < indexY) return -1;
                //else if (indexX == indexY) return 0;
                //else return 1;
            }
        }

        private class DefaultGroupsComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return String.Compare(x, y);
            }
        }
    }

    public class GroupViewModel<ItemType> : ObservableCollection<ItemType>
        where ItemType : class
    {

        public GroupViewModel()
            : base()
        {
            base.CollectionChanged += new NotifyCollectionChangedEventHandler(GroupViewModel_CollectionChanged);
        }

        void GroupViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasItems = base.Count != 0;
        }

        public const string HasItemsPropertyName = "HasItems";
        private bool hatItems = false;
        public bool HasItems
        {
            get
            {
                return hatItems;
            }

            set
            {
                if (hatItems == value)
                {
                    return;
                }
                hatItems = value;
                RaisePropertyChanged(HasItemsPropertyName);
            }
        }

        public const string KeyPropertyName = "Key";
        private string key = "";

        public string Key
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

        void RaisePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            this.OnPropertyChanged(e);
        }

    }

}
