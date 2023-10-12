namespace Radical.Series
{
    using System.Collections;
    using System.Collections.Generic;

    public class SeriesItemUniqueKeyEnumerator<V> : IEnumerator<ulong>, IEnumerator
    {
        public ISeriesItem<V> Entry;
        private ISeries<V> map;

        public SeriesItemUniqueKeyEnumerator(ISeries<V> Map)
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

        ulong IEnumerator<ulong>.Current => Entry.Key;

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
