namespace Radical.Series.Base
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Instant;
    using Uniques;
    using Invoking;
    using Instant.Updating;

    public class BaseTracedRegistry<V>
        : RegistrySeries<V>,
            ITracedSeries,
            INotifyPropertyChanged,
            INotifyPropertyChanging,
            INotifyCollectionChanged where V : class, ITracedSeries
    {
        int readers;

        public BaseTracedRegistry() : this(false, 17, HashBits.bit64) { }

        public BaseTracedRegistry(int capacity = 17, HashBits bits = HashBits.bit64)
            : base(capacity, bits)
        {
            Initialize();
        }

        public BaseTracedRegistry(bool repeatable, int capacity = 17, HashBits bits = HashBits.bit64)
            : base(repeatable, capacity, bits)
        {
            Initialize();
        }

        public BaseTracedRegistry(
            IEnumerable<IUnique<V>> collection,
            int capacity = 17,
            bool repeatable = false,
            HashBits bits = HashBits.bit64
        ) : this(repeatable, capacity, bits)
        {
            Initialize();
            if (collection != null)
                foreach (IUnique<V> c in collection)
                    Add(c);
        }

        public BaseTracedRegistry(
            IEnumerable<V> collection,
            int capacity = 17,
            bool repeatable = false,
            HashBits bits = HashBits.bit64
        ) : this(repeatable, capacity, bits)
        {
            Initialize();
            if (collection != null)
                foreach (V c in collection)
                    Add(c);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        protected void AddNotifier(V itemAdded)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemAdded)
            );
        }

        protected void AddNotifier(IEnumerable<V> itemsAdded)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsAdded)
            );
        }

        protected void ChangeNotiifer(V newItem, V oldItem)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newItem,
                    oldItem
                )
            );
        }

        protected override ISeriesItem<V> GetItem(ulong key, V item)
        {
            ISeriesItem<V> _item = base.GetItem(key, item);

            return _item;
        }

        protected override int IndexOf(ulong key, V item)
        {
            int id = 0;

            id = base.IndexOf(key, item);

            return id;
        }

        protected override V InnerGet(ulong key)
        {
            V v = base.InnerGet(key);

            return v;
        }

        protected override ISeriesItem<V> InnerGetItem(ulong key)
        {
            return base.InnerGetItem(key);
        }

        protected override ISeriesItem<V> InnerPut(ISeriesItem<V> value)
        {
            int _count = count;

            ISeriesItem<V> temp = base.InnerPut(value);

            return temp;
        }

        protected override ISeriesItem<V> InnerPut(V value)
        {
            ISeriesItem<V> temp = base.InnerPut(value);

            return temp;
        }

        protected override ISeriesItem<V> InnerPut(ulong key, V value)
        {
            ISeriesItem<V> temp = base.InnerPut(key, value);

            return temp;
        }

        protected override V InnerRemove(ulong key)
        {
            V temp = base.InnerRemove(key);
            RemoveNotifier(temp);
            return temp;
        }

        protected override ISeriesItem<V> InnerSet(ISeriesItem<V> value)
        {
            ISeriesItem<V> item = InnerGetItem(value.Key);
            if (item != null)
            {
                V oldItem = item.Value;
                V newItem = value.Value;
                item.Value = newItem;
                ChangeNotiifer(newItem, oldItem);
            }
            return item;
        }

        protected override ISeriesItem<V> InnerSet(V value)
        {
            return InnerSet(unique.Key(value), value);
        }

        protected override ISeriesItem<V> InnerSet(ulong key, V value)
        {
            ISeriesItem<V> item = InnerGetItem(key);
            if (item != null)
            {
                V oldItem = item.Value;
                V newItem = value;
                item.Value = newItem;
                ChangeNotiifer(newItem, oldItem);
            }
            return item;
        }

        protected override bool InnerTryGet(ulong key, out ISeriesItem<V> output)
        {
            return base.InnerTryGet(key, out output);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged.Invoke(this, e);
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Invoke(sender, e);
        }

        protected void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            PropertyChanging.Invoke(sender, e);
        }

        protected override void Rehash(int newsize)
        {
            base.Rehash(newsize);
        }

        protected override void Reindex()
        {
            base.Reindex();
        }

        protected void RemoveNotifier(V itemRemoved)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    itemRemoved
                )
            );
        }

        protected void ReplaceNotifier(V itemsMoved)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemsMoved)
            );
        }

        protected void ResetNotifier(IEnumerable<V> itemsReset)
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset,
                    itemsReset
                )
            );
        }

        protected override bool InnerAdd(ISeriesItem<V> value)
        {
            bool temp = base.InnerAdd(value);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, temp)
            );

            return temp;
        }

        protected override bool InnerAdd(V value)
        {
            bool temp = base.InnerAdd(value);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, temp)
            );

            return temp;
        }

        protected override bool InnerAdd(ulong key, V value)
        {
            bool temp = base.InnerAdd(key, value);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, temp)
            );

            return temp;
        }

        public override void Clear()
        {
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
            base.Clear();
        }

        public override void CopyTo(Array array, int index)
        {
            base.CopyTo(array, index);
        }

        public override void CopyTo(ISeriesItem<V>[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public override void CopyTo(V[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public override V Dequeue()
        {
            V temp = base.Dequeue();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, temp)
            );

            return temp;
        }

        public override ISeriesItem<V> EmptyItem()
        {
            return new TracedSeriesItem<V>();
        }

        public override ISeriesItem<V>[] EmptyTable(int size)
        {
            return new TracedSeriesItem<V>[size];
        }

        public override ISeriesItem<V>[] EmptyVector(int size)
        {
            return new TracedSeriesItem<V>[size];
        }

        public override ISeriesItem<V> GetItem(int index)
        {
            if (index < count)
            {
                if (removed > 0)
                {
                    Reindex();
                }

                ISeriesItem<V> temp = vector[index];

                return temp;
            }
            throw new IndexOutOfRangeException("Index out of range");
        }

        public override int IndexOf(ISeriesItem<V> item)
        {
            int id = 0;

            id = base.IndexOf(item);

            return id;
        }

        public void Initialize()
        {
            NoticeChange = new Invoker<BaseTracedRegistry<V>>(this, a => nameof(a.OnPropertyChanged));
            NoticeChanging = new Invoker<BaseTracedRegistry<V>>(
                this,
                a => nameof(a.OnPropertyChanging)
            );
        }

        public override void Insert(int index, ISeriesItem<V> item)
        {
            InnerInsert(index, item);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item.Value)
            );
        }

        public override ISeriesItem<V> NewItem(ISeriesItem<V> item)
        {
            return new TracedSeriesItem<V>(item);
        }

        public override ISeriesItem<V> NewItem(V value)
        {
            return new TracedSeriesItem<V>(value);
        }

        public override ISeriesItem<V> NewItem(object key, V value)
        {
            return new TracedSeriesItem<V>(key, value);
        }

        public override ISeriesItem<V> NewItem(ulong key, V value)
        {
            return new TracedSeriesItem<V>(key, value);
        }

        public override V[] ToArray()
        {
            V[] array = base.ToArray();

            return array;
        }

        public override bool TryDequeue(out ISeriesItem<V> output)
        {
            bool temp = base.TryDequeue(out output);
            RemoveNotifier(output.Value);
            return temp;
        }

        public override bool TryDequeue(out V output)
        {
            bool temp = base.TryDequeue(out output);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, temp)
            );

            return temp;
        }

        public override bool TryPick(int skip, out V output)
        {
            bool temp = base.TryPick(skip, out output);

            return temp;
        }

        public IInvoker NoticeChange { get; set; }

        public IInvoker NoticeChanging { get; set; }

        public IUpdater Updater { get; set; }
    }
}
