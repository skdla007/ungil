namespace ArcGISControl.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// ObservableCollection의 기능을 확장하는 클래스이다.
    /// 불필요하게 CollectionChanged 이벤트가 많이 불리는 것을 방지하기 위한 것이 목적이다.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public ExtendedObservableCollection() { }
        public ExtendedObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public ExtendedObservableCollection(List<T> list) : base(list) { }

        public void AddRange(IEnumerable<T> elements)
        {
            this.CheckReentrancy();

            var innerItems = this.Items;
            var innerList = innerItems as List<T>;

            if (innerList != null)
            {
                innerList.AddRange(elements);
            }
            else
            {
                foreach (var element in this == elements ? elements.ToList() : elements)
                {
                    innerItems.Add(element);
                }
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        public void InsertRange(int index, IEnumerable<T> elements)
        {
            this.CheckReentrancy();

            var innerItems = this.Items;
            var innerList = innerItems as List<T>;

            if (innerList != null)
            {
                innerList.InsertRange(index, elements);
            }
            else
            {
                foreach (var element in this == elements ? elements.ToList() : elements)
                {
                    innerItems.Insert(index++, element);
                }
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        /// <summary>
        /// Clears the contents of the collection, and Add <paramref name="elements"/> to the collection.
        /// </summary>
        /// <param name="elements">
        /// The enumerable whose elements should be added to the end of the collection.
        /// The enumerable itself cannot be null, but it can contain elements that are
        /// null, if type <typeparamref name="T"/> is a reference type.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="elements"/> is null.</exception>
        public void ReplaceAll(IEnumerable<T> elements)
        {
            if (Equals(elements, this))
                return;

            if (elements == null)
                throw new ArgumentNullException("elements");

            this.CheckReentrancy();

            var innerItems = this.Items;
            var innerList = innerItems as List<T>;

            innerItems.Clear();
            if (innerList != null)
            {
                innerList.AddRange(elements);
            }
            else
            {
                foreach (var element in elements)
                {
                    innerItems.Add(element);
                }
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        public void Sort()
        {
            this.Sort(0, this.Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            this.Sort(0, this.Count, comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            this.CheckReentrancy();

            var innerItems = this.Items;
            var innerList = innerItems as List<T>;
            if (innerList != null)
            {
                innerList.Sort(index, count, comparer);
            }
            else
            {
                var array = innerItems.ToArray();
                Array.Sort(array, index, count, comparer);

                for (var i = 0; i < array.Length; i++)
                {
                    innerItems[i] = array[i];
                }
            }

            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        public void Sort(Comparison<T> comparison)
        {
            this.CheckReentrancy();

            var innerItems = this.Items;
            var innerList = innerItems as List<T>;
            if (innerList != null)
            {
                innerList.Sort(comparison);
            }
            else
            {
                var array = innerItems.ToArray();
                Array.Sort(array, comparison);

                for (var i = 0; i < array.Length; i++)
                {
                    innerItems[i] = array[i];
                }
            }

            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        public void RemoveAll(Predicate<T> match)
        {
            this.CheckReentrancy();

            var innerItems = this.Items;

            var innerList = innerItems as List<T>;
            if (innerList != null)
            {
                innerList.RemoveAll(match);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return;
            }

            var copyList = this.Items.ToList();
            copyList.RemoveAll(match);

            this.Items.Clear();
            foreach (var item in copyList)
            {
                this.Items.Add(item);
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }


        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }
}
