namespace Radical.Series.Enumerators
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SeriesItemEnumerator<V> : IEnumerator<ISeriesItem<V>>, IEnumerator<V>, IEnumerator
    {
        public ISeriesItem<V> Entry;
        private ISeries<V> map;

        public SeriesItemEnumerator(ISeries<V> Map)
        {
            map = Map;
            Entry = map.First;
        }

        public object Current => Entry.Value;

        public int Index => Entry.Index;

        public ulong Key => Entry.Key;

        public V Value => Entry.Value;

        ISeriesItem<V> IEnumerator<ISeriesItem<V>>.Current => Entry;

        V IEnumerator<V>.Current => Entry.Value;

        public void Dispose()
        {
            Entry = map.First;
        }

        public bool MoveNext()
        {
            Entry = map.Next(Entry);
            if (Entry != null)
            {
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Entry = map.First;
        }
    }

    public class ItemSeriesAsync<V> : IAsyncEnumerator<ISeriesItem<V>>, IAsyncEnumerator<V>, IEnumerator
    {
        public ISeriesItem<V> Entry;
        private ISeries<V> map;

        public ItemSeriesAsync(ISeries<V> Map)
        {
            map = Map;
            Entry = map.First;
        }

        public object Current => Entry.Value;

        public int Index => Entry.Index;

        public ulong Key => Entry.Key;

        public V Value => Entry.Value;

        ISeriesItem<V> IAsyncEnumerator<ISeriesItem<V>>.Current => Entry;

        V IAsyncEnumerator<V>.Current => Entry.Value;

        public ValueTask DisposeAsync()
        {            
            Entry = map.First;
            return ValueTask.CompletedTask;
        }

        public bool MoveNext()
        {
            Entry = map.Next(Entry);
            if (Entry != null)
            {
                return true;
            }
            return false;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            
            Entry = map.Next(Entry);
            if (Entry != null)
            {
                return ValueTask.FromResult(true);
            }
            return ValueTask.FromResult(false);
        }

        public void Reset()
        {
            Entry = map.First;
        }
    }
}
