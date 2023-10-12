namespace Radical.Series.Enumerators
{
    using System.Collections;
    using System.Collections.Generic;
    using Uniques;

    public class SeriesItemKeyEnumerator<V> : IEnumerator<IUnique<V>>, IEnumerator
    {
        public ISeriesItem<V> Entry;
        private ISeries<V> map;

        public SeriesItemKeyEnumerator(ISeries<V> Map)
        {
            map = Map;
            Entry = map.First;
        }

        public object Current => Entry.Key;

        public ulong Key
        {
            get { return Entry.Key; }
        }

        public V Value
        {
            get { return Entry.Value; }
        }

        IUnique<V> IEnumerator<IUnique<V>>.Current => Entry;

        public void Dispose()
        {
            Entry = map.First;
        }

        public bool MoveNext()
        {
            Entry = Entry.Next;
            if (Entry != null)
            {
                if (Entry.Removed)
                    return MoveNext();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Entry = map.First;
        }
    }
}
