using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;

namespace OneShoppingList.ListUtils
{
    public class LazyList<T> : IList, INotifyCollectionChanged where T : class
    {
        private IEnumerator<T> myEnumerator = null;
        private List<T> myInternalList = new List<T>();

        public LazyList()
        {
            myEnumerator = new List<T>().GetEnumerator();
        }

        public LazyList(IEnumerable<T> enumerator)
        {
            Attach(enumerator);
        }

        private void AppendNextItemIfAvailable()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (myEnumerator.MoveNext())
                {
                    myInternalList.Add(myEnumerator.Current);
                    if (this.CollectionChanged != null)
                    {
                        this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, myEnumerator.Current, this.Count - 1));
                    }
                }
            });
        }

        public void Attach(IEnumerable<T> enumerator)
        {
            int firstIndexToBeRemoved = Math.Min(5, myInternalList.Count);

            myEnumerator = enumerator.GetEnumerator();
            int i;
            for (i = 0; i < firstIndexToBeRemoved && myEnumerator.MoveNext(); i++)
            {
                myInternalList[i] = myEnumerator.Current;
            }
            if (i < firstIndexToBeRemoved)
            {
                firstIndexToBeRemoved = i;
            }
            int numberOfItemsToBeRemoved = myInternalList.Count - firstIndexToBeRemoved;
            if (numberOfItemsToBeRemoved > 0)
            {
                myInternalList.RemoveRange(firstIndexToBeRemoved, numberOfItemsToBeRemoved);
            }

            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            AppendNextItemIfAvailable();
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get
            {
                T result = myInternalList[index];
                if (index == myInternalList.Count - 1)
                {
                    AppendNextItemIfAvailable();
                }
                return result;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return myInternalList.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
