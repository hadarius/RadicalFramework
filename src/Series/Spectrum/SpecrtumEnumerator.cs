namespace Radical.Series.Spectrum
{
    using System.Collections;
    using System.Collections.Generic;
    using Base;

    public class SpectrumEnumerator<V> : IEnumerator<ISeriesItem<V>>, IEnumerator
    {
        public ISeriesItem<V> Entry;
        private int iterated = 0;
        private int lastReturned;
        private Spectrum<V> map;

        public SpectrumEnumerator(Spectrum<V> Map)
        {
            map = Map;
            Entry = new SeriesItem64<V>();
        }

        public object Current => Entry;

        public int Key
        {
            get { return (int)Entry.Key; }
        }

        public V Value
        {
            get { return Entry.Value; }
        }

        ISeriesItem<V> IEnumerator<ISeriesItem<V>>.Current => Entry;

        public void Dispose()
        {
            iterated = 0;
            Entry = null;
        }

        public bool HasNext()
        {
            return iterated < map.Count;
        }

        public bool MoveNext()
        {
            return Next();
        }

        public bool Next()
        {
            if (!HasNext())
            {
                return false;
            }

            if (iterated == 0)
            {
                lastReturned = map.IndexMin;
                iterated++;
                Entry.Key = (uint)lastReturned;
                Entry.Value = map.Get(lastReturned);
            }
            else
            {
                lastReturned = map.Next(lastReturned);
                iterated++;
                Entry.Key = (uint)lastReturned;
                Entry.Value = map.Get(lastReturned);
            }
            return true;
        }

        public void Reset()
        {
            Entry = new SeriesItem64<V>();
            iterated = 0;
        }
    }
}
