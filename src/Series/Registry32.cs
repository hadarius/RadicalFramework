namespace Radical.Series
{
    using System.Collections.Generic;
    using Radical.Uniques;
    using Base;

    public class Registry32<V> : BaseRegistry<V>
    {
        public Registry32(
            IEnumerable<IUnique<V>> collection,
            int capacity = 17,
            bool repeatable = false
        ) : this(repeatable, capacity)
        {
            foreach (var c in collection)
                this.Add(c);
        }

        public Registry32(IEnumerable<V> collection, int capacity = 17, bool repeatable = false)
            : this(repeatable, capacity)
        {
            foreach (var c in collection)
                this.Add(c);
        }

        public Registry32(bool repeatable = false, int capacity = 17)
            : base(repeatable, capacity, HashBits.bit32) { }

        public override ISeriesItem<V>[] EmptyVector(int size)
        {
            return new SeriesItem32<V>[size];
        }

        public override ISeriesItem<V> EmptyItem()
        {
            return new SeriesItem32<V>();
        }

        public override ISeriesItem<V>[] EmptyTable(int size)
        {
            return new SeriesItem32<V>[size];
        }

        public override ISeriesItem<V> NewItem(ISeriesItem<V> item)
        {
            return new SeriesItem32<V>(item);
        }

        public override ISeriesItem<V> NewItem(object key, V value)
        {
            return new SeriesItem32<V>(key, value);
        }

        public override ISeriesItem<V> NewItem(ulong key, V value)
        {
            return new SeriesItem32<V>(key, value);
        }

        public override ISeriesItem<V> NewItem(V value)
        {
            return new SeriesItem32<V>(value);
        }
    }
}
