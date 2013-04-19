using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace OneShoppingList.Model
{
    [DataContract]
    public abstract class SyncItemBase : INotifyPropertyChanged
    {
        public SyncItemBase()
        {
            this.Key = Guid.NewGuid();
            this.TimeStamp = DateTime.Now;
        }

        [DataMember]
        public Guid Key { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        [DataMember]
        public bool isDeleted = false;
        public const string IsDeletedPropertyName = "IsDeleted";
        public bool IsDeleted
        {
            get
            {
                return isDeleted;
            }
            set
            {
                if (isDeleted == value)
                {
                    return;
                }
                isDeleted = value;
                this.TimeStamp = DateTime.Now;
                RaisePropertyChanged(IsDeletedPropertyName);
            }
        }

        public virtual void Update(SyncItemBase source)
        {
            this.Key = source.Key;
            this.IsDeleted = source.IsDeleted;
            this.TimeStamp = source.TimeStamp;
        }

        public abstract SyncItemBase Clone();

    }

    public class ListSync
    {
        /// <summary>
        /// Syncs the two lists based on the time stamps of their items. baseTimeStamp is the DateTime when the two lists were synced last time.
        /// </summary>
        /// <typeparam name="TItem">The actual type of the list items.</typeparam>
        /// <param name="localList">The list available on the local device (cached after the last sync)</param>
        /// <param name="remoteList">The list from the server (probably updated by other devices since the last sync)</param>
        /// <param name="baseTimeStamp">The DateTime of the last sync.</param>
        /// <returns></returns>
        static public bool SyncLists<TItem>(IList<TItem> localList, IList<TItem> remoteList) where TItem : SyncItemBase
        {
            bool remoteListChanged = false;
            bool localListChanged = false;

            OneWaySync<TItem>(localList, remoteList, ref localListChanged, ref remoteListChanged);
            OneWaySync<TItem>(remoteList, localList, ref remoteListChanged, ref localListChanged);

            return remoteListChanged;
        }

        private static void OneWaySync<TItem>(IList<TItem> sourceList, IList<TItem> targetList, ref bool sourceListChanged, ref bool targetListChanged) where TItem : SyncItemBase
        {
            var oldDeletedItems = sourceList.Where(item => item.IsDeleted && item.TimeStamp < DateTime.Now - TimeSpan.FromDays(14)).ToList();
            foreach (TItem ti in oldDeletedItems)
            {
                sourceList.Remove(ti);
                sourceListChanged = true;
            }

            foreach (SyncItemBase source in sourceList)
            {
                SyncItemBase target = targetList.Where(t => source.Key == t.Key).SingleOrDefault();
                if (target == null)
                {
                    targetList.Add(source.Clone() as TItem);
                    targetListChanged = true;
                }
                else
                {
                    if (source.TimeStamp > target.TimeStamp)
                    {
                        target.Update(source);
                        targetListChanged = true;
                    }
                }
            }
        }
    }
}
