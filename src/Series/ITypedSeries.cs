using System.Collections.Generic;

namespace Radical.Series
{
    using Uniques;

    public interface ITypedSeries<V> : ISeries<V> where V : IUnique
    {
        V this[object key, ulong seed] { get; set; }
        V this[IUnique key, ulong seed] { get; set; }

        bool ContainsKey(object key, ulong seed);

        bool Contains(V item, ulong seed);

        V Get(object key, ulong seed);

        bool TryGet(object key, ulong seed, out ISeriesItem<V> output);
        bool TryGet(object key, ulong seed, out V output);

        ISeriesItem<V> GetItem(object key, ulong seed);

        ISeriesItem<V> New(object key, ulong seed);

        bool Add(object key, ulong seed, V value);
        bool Add(V value, ulong seed);
        void Add(IList<V> items, ulong seed);
        void Add(IEnumerable<V> items, ulong seed);

        bool Enqueue(object key, ulong seed, V value);
        bool Enqueue(V item, ulong seed);

        ISeriesItem<V> Put(object key, ulong seed, V value);
        ISeriesItem<V> Put(object key, ulong seed, object value);
        void Put(IList<V> items, ulong seed);
        void Put(IEnumerable<V> items, ulong seed);
        ISeriesItem<V> Put(V value, ulong seed);

        V Remove(object key, ulong seed);
        bool TryRemove(object key, ulong seed);

        ISeriesItem<V> NewItem(V value, ulong seed);
        ISeriesItem<V> NewItem(object key, ulong seed, V value);
        ISeriesItem<V> NewItem(ulong key, ulong seed, V value);
    }
}
