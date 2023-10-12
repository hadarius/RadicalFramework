namespace Radical.Series
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Uniques;

    public interface ISeries<V>
        : IEnumerable<V>,
            IEnumerable,
            ICollection,
            ICollection<V>,
            IList<V>,
            IProducerConsumerCollection<V>,
            IDisposable,
            IUnique,
            IFindableSeries<V>
    {
        ISeriesItem<V> First { get; }
        ISeriesItem<V> Last { get; }

        bool IsRepeatable { get; }

        ISeriesItem<V> Next(ISeriesItem<V> item);

        new int Count { get; }
        int MinCount { get; set; }

        bool ContainsKey(ulong key);
        bool ContainsKey(object key);
        bool ContainsKey(IUnique key);

        bool Contains(ISeriesItem<V> item);
        bool Contains(IUnique<V> item);
        bool Contains(ulong key, V item);

        V Get(object key);
        V Get(ulong key);
        V Get(IUnique key);
        V Get(IUnique<V> key);

        bool TryGet(object key, out ISeriesItem<V> output);
        bool TryGet(object key, out V output);
        bool TryGet(ulong key, out V output);
        bool TryGet(IUnique key, out ISeriesItem<V> output);
        bool TryGet(IUnique<V> key, out ISeriesItem<V> output);

        ISeriesItem<V> GetItem(object key);
        ISeriesItem<V> GetItem(ulong key);
        ISeriesItem<V> GetItem(IUnique key);
        ISeriesItem<V> GetItem(IUnique<V> key);

        ISeriesItem<V> Set(object key, V value);
        ISeriesItem<V> Set(ulong key, V value);
        ISeriesItem<V> Set(IUnique key, V value);
        ISeriesItem<V> Set(IUnique<V> key, V value);
        ISeriesItem<V> Set(V value);
        ISeriesItem<V> Set(IUnique<V> value);
        ISeriesItem<V> Set(ISeriesItem<V> value);
        int Set(IEnumerable<V> values);
        int Set(IList<V> values);
        int Set(IEnumerable<ISeriesItem<V>> values);
        int Set(IEnumerable<IUnique<V>> values);

        ISeriesItem<V> EnsureGet(object key, Func<ulong, V> ensureaction);
        ISeriesItem<V> EnsureGet(ulong key, Func<ulong, V> ensureaction);
        ISeriesItem<V> EnsureGet(IUnique key, Func<ulong, V> ensureaction);
        ISeriesItem<V> EnsureGet(IUnique<V> key, Func<ulong, V> ensureaction);

        ISeriesItem<V> New();
        ISeriesItem<V> New(ulong key);
        ISeriesItem<V> New(object key);

        bool Add(object key, V value);
        bool Add(ulong key, V value);
        void Add(ISeriesItem<V> item);
        void Add(IList<ISeriesItem<V>> itemList);
        void Add(IEnumerable<ISeriesItem<V>> items);
        void Add(IList<V> items);
        void Add(IEnumerable<V> items);
        void Add(IUnique<V> items);
        void Add(IList<IUnique<V>> items);
        void Add(IEnumerable<IUnique<V>> items);

        bool Enqueue(object key, V value);
        void Enqueue(ISeriesItem<V> item);
        bool Enqueue(V item);

        V Dequeue();
        bool TryDequeue(out ISeriesItem<V> item);
        bool TryDequeue(out V item);
        new bool TryTake(out V item);

        bool TryPick(int skip, out V output);

        ISeriesItem<V> Put(object key, V value);
        ISeriesItem<V> Put(ulong key, V value);
        ISeriesItem<V> Put(ISeriesItem<V> item);
        void Put(IList<ISeriesItem<V>> itemList);
        void Put(IEnumerable<ISeriesItem<V>> items);
        void Put(IList<V> items);
        void Put(IEnumerable<V> items);
        ISeriesItem<V> Put(V value);
        ISeriesItem<V> Put(IUnique<V> items);
        void Put(IList<IUnique<V>> items);
        void Put(IEnumerable<IUnique<V>> items);

        V Remove(object key);
        bool Remove(object key, V item);
        bool Remove(ISeriesItem<V> item);
        bool Remove(IUnique<V> item);
        bool TryRemove(object key);

        void Renew(IEnumerable<V> items);
        void Renew(IList<V> items);
        void Renew(IList<ISeriesItem<V>> items);
        void Renew(IEnumerable<ISeriesItem<V>> items);

        new V[] ToArray();

        IEnumerable<ISeriesItem<V>> AsItems();

        IEnumerable<V> AsValues();

        new void CopyTo(Array array, int arrayIndex);

        new bool IsSynchronized { get; set; }
        new object SyncRoot { get; set; }

        ISeriesItem<V> NewItem(V value);
        ISeriesItem<V> NewItem(object key, V value);
        ISeriesItem<V> NewItem(ulong key, V value);
        ISeriesItem<V> NewItem(ISeriesItem<V> item);

        void CopyTo(ISeriesItem<V>[] array, int destIndex);
        void CopyTo(IUnique<V>[] array, int arrayIndex);

        new void Clear();

        void Resize(int size);

        void Flush();
    }
}
