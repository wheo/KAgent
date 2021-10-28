using KAgent.Interface;
using System.Collections.ObjectModel;

namespace KAgent.Singleton
{
    internal class Status
    {
        public LimitedSizeObservableCollection<Item> items { get; set; }

        public class LimitedSizeObservableCollection<T> : ObservableCollection<T>
        {
            public int Capacity { get; }

            public LimitedSizeObservableCollection(int capacity)
            {
                Capacity = capacity;
            }

            public new void Add(T item)
            {
                if (Count >= Capacity)
                {
                    this.RemoveAt(0);
                }
                base.Add(item);
            }

            public new void InsertItem(int index, T item)
            {
                if (Count >= Capacity)
                {
                    this.RemoveAt(Count - 1);
                }
                base.Insert(index, item);
            }
        }

        private Status()
        {
            if (items == null)
            {
                items = new LimitedSizeObservableCollection<Item>(1000);
            }
        }

        private static Status instance = null;

        public static Status GetInstance()
        {
            if (instance == null)
            {
                instance = new Status();
            }
            return instance;
        }
    }
}