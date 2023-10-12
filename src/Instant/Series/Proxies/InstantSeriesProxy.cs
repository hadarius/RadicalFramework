namespace Radical.Instant.Series.Proxies
{
    using Ethernet.Transit;
    using Instant.Proxies;
    using Math;
    using Querying;
    using Radical.Instant.Series;
    using Radical.Series;
    using Radical.Uniques;
    using Rubrics;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public abstract class InstantSeriesProxy : IInstantSeriesProxy
    {
        public IInstantCreator Creator { get; set; }

        public bool Prime { get; set; }

        public abstract object this[int index, string propertyName] { get; set; }

        public abstract object this[int index, int fieldId] { get; set; }

        public abstract IInstantSeries Proxies { get; set; }

        public abstract IInstantSeries Series { get; set; }

        public IQueryable<IInstant> View
        {
            get => Proxies.View;
            set => Proxies.View = value;
        }

        public InstantSeriesFilter Filter
        {
            get => Proxies.Filter;
            set => Proxies.Filter = value;
        }
        public InstantSeriesSort Sort
        {
            get => Proxies.Sort;
            set => Proxies.Sort = value;
        }
        public Func<IInstant, bool> Predicate
        {
            get => Proxies.Predicate;
            set => Proxies.Predicate = value;
        }

        public virtual bool IsRepeatable
        {
            get => false;
        }

        public int Serialize(
            Stream tostream,
            int offset,
            int batchSize
        )
        {
            throw new NotImplementedException();
        }

        public int Serialize(
            ITransitBuffer buffor,
            int offset,
            int batchSize
        )
        {
            throw new NotImplementedException();
        }

        public object Deserialize(
            Stream fromstream
        )
        {
            throw new NotImplementedException();
        }

        public object Deserialize(
            ITransitBuffer buffer
        )
        {
            throw new NotImplementedException();
        }

        public object[] GetMessage()
        {
            return new[] { this };
        }

        public object GetHeader()
        {
            return Series;
        }

        public void Clear()
        {
            Proxies.Clear();
        }

        public void Flush()
        {
            Proxies.Flush();
        }

        public IInstant NewInstant()
        {
            return Series.NewInstant();
        }

        public IProxy NewProxy()
        {
            return Series.NewProxy();
        }

        public ISeriesItem<IInstant> Next(ISeriesItem<IInstant> item)
        {
            return Proxies.Next(item);
        }

        public bool ContainsKey(object key)
        {
            return Proxies.ContainsKey(key);
        }

        public bool ContainsKey(IUnique key)
        {
            return Proxies.ContainsKey(key);
        }

        public bool Contains(IInstant item)
        {
            return Proxies.Contains(item);
        }

        public bool Contains(ISeriesItem<IInstant> item)
        {
            return Proxies.Contains(item);
        }

        public bool Contains(IUnique<IInstant> item)
        {
            return Proxies.Contains(item);
        }

        public virtual bool Contains(ulong key, IInstant item)
        {
            return Proxies.Contains(key, item);
        }

        public IInstant Get(object key)
        {
            return Proxies.Get(key);
        }

        public IInstant Get(IUnique<IInstant> key)
        {
            return Proxies.Get(key);
        }

        public IInstant Get(IUnique key)
        {
            return Proxies.Get(key);
        }

        public bool TryGet(object key, out ISeriesItem<IInstant> output)
        {
            return Proxies.TryGet(key, out output);
        }

        public bool TryGet(object key, out IInstant output)
        {
            return Proxies.TryGet(key, out output);
        }

        public ISeriesItem<IInstant> GetItem(object key)
        {
            return Proxies.GetItem(key);
        }

        public ISeriesItem<IInstant> New()
        {
            ISeriesItem<IInstant> item = Series.New();
            if (item != null)
            {
                Proxies.Add(item);
            }
            return item;
        }

        public ISeriesItem<IInstant> New(ulong key)
        {
            ISeriesItem<IInstant> item = Series.New(key);
            if (item != null)
            {
                Proxies.Add(item);
            }
            return item;
        }

        public ISeriesItem<IInstant> New(object key)
        {
            ISeriesItem<IInstant> item = Series.New();
            if (item != null)
            {
                Proxies.Add(item);
            }
            return item;
        }

        public bool Add(ulong key, IInstant item)
        {
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Add(key, _item);
            }
            else
            {
                return Proxies.TryAdd(Series.Put(item).Value);
            }
        }

        public void Add(IInstant item)
        {
            ulong key = item.Key;
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                Proxies.Add(key, _item);
            }
            else
            {
                Proxies.Add(Series.Put(item).Value);
            }
        }

        public bool Add(object key, IInstant item)
        {
            IInstant _item;
            ulong _key = key.UniqueKey64();
            if (Series.TryGet(_key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Add(_key, _item);
            }
            else
                return Proxies.TryAdd(Series.Put(item).Value);
        }

        public void Add(ISeriesItem<IInstant> item)
        {
            ulong key = item.Key;
            ISeriesItem<IInstant> _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item.Value, item.Value))
                {
                    _item.Value.ValueArray = item.Value.ValueArray;
                    _item.Key = item.Key;
                }
                Proxies.Add(_item);
            }
            else
                Proxies.Add(Series.Put(item));
        }

        public void Add(IList<ISeriesItem<IInstant>> itemList)
        {
            foreach (var item in itemList)
                Proxies.Add(item);
        }

        public void Add(IEnumerable<ISeriesItem<IInstant>> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Add(IList<IInstant> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Add(IEnumerable<IInstant> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Add(IUnique<IInstant> item)
        {
            IInstant _item;
            if (Series.TryGet(item, out _item))
            {
                IInstant value = item.UniqueObject;
                if (!ReferenceEquals(_item, value))
                {
                    _item.ValueArray = value.ValueArray;
                    _item.Key = value.Key;
                }
                Proxies.Add(_item);
            }
            else
                Proxies.Add(Series.Put(item).Value);
        }

        public void Add(IList<IUnique<IInstant>> items)
        {
            foreach (IUnique<IInstant> item in items)
                Add(item);
        }

        public void Add(IEnumerable<IUnique<IInstant>> items)
        {
            foreach (IUnique<IInstant> item in items)
                Add(item);
        }

        public bool TryAdd(IInstant item)
        {
            ulong key = item.Key;
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Add(key, _item);
            }
            else
                return Proxies.TryAdd(Series.Put(item).Value);
        }

        public bool Enqueue(object key, IInstant value)
        {
            return Proxies.Enqueue(key, value);
        }

        public void Enqueue(ISeriesItem<IInstant> item)
        {
            Proxies.Enqueue(item);
        }

        public bool Enqueue(IInstant item)
        {
            return Proxies.Enqueue(item);
        }

        public IInstant Dequeue()
        {
            return Proxies.Dequeue();
        }

        public bool TryDequeue(out ISeriesItem<IInstant> item)
        {
            return Proxies.TryDequeue(out item);
        }

        public bool TryDequeue(out IInstant item)
        {
            return Proxies.TryDequeue(out item);
        }

        public bool TryTake(out IInstant item)
        {
            return Proxies.TryTake(out item);
        }

        public ISeriesItem<IInstant> Put(ulong key, IInstant item)
        {
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Put(key, _item);
            }
            else
                return Proxies.Put(Series.Put(key, item).Value);
        }

        public ISeriesItem<IInstant> Put(object key, IInstant item)
        {
            ulong _key = key.UniqueKey();
            IInstant _item;
            if (Series.TryGet(_key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Put(_key, _item);
            }
            else
                return Proxies.Put(Series.Put(key, item).Value);
        }

        public ISeriesItem<IInstant> Put(ISeriesItem<IInstant> seriesItem)
        {
            IInstant figure = seriesItem.Value;
            IInstant _item;
            if (Series.TryGet(figure.Key, out _item))
            {
                if (!ReferenceEquals(_item, figure))
                {
                    _item.ValueArray = figure.ValueArray;
                    _item.Key = figure.Key;
                }
                return Proxies.Put(figure);
            }
            else
                return Proxies.Put(Series.Put(figure));
        }

        public void Put(IList<ISeriesItem<IInstant>> itemList)
        {
            foreach (var item in itemList)
                Put(item);
        }

        public void Put(IEnumerable<ISeriesItem<IInstant>> items)
        {
            foreach (var item in items)
                Put(item);
        }

        public void Put(IList<IInstant> items)
        {
            foreach (var item in items)
                Put(item);
        }

        public void Put(IEnumerable<IInstant> items)
        {
            foreach (var item in items)
                Put(item);
        }

        public ISeriesItem<IInstant> Put(IInstant item)
        {
            ulong key = item.Key;
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Put(key, _item);
            }
            else
                return Proxies.Put(Series.Put(item).Value);
        }

        public ISeriesItem<IInstant> Put(IUnique<IInstant> value)
        {
            ulong key = value.Key;
            IInstant item = value.UniqueObject;
            IInstant _item;
            if (Series.TryGet(key, out _item))
            {
                if (!ReferenceEquals(_item, item))
                {
                    _item.ValueArray = item.ValueArray;
                    _item.Key = item.Key;
                }
                return Proxies.Put(key, _item);
            }
            else
                return Proxies.Put(Series.Put(item).Value);
        }

        public void Put(IList<IUnique<IInstant>> items)
        {
            foreach (var item in items)
                Put(item);
        }

        public void Put(IEnumerable<IUnique<IInstant>> items)
        {
            foreach (var item in items)
                Put(item);
        }

        public IInstant Remove(object key)
        {
            return Proxies.Remove(key);
        }

        public bool Remove(IInstant item)
        {
            if (Proxies.Remove(item) != null)
                return true;
            return false;
        }

        public bool Remove(ISeriesItem<IInstant> item)
        {
            return Proxies.Remove(item);
        }

        public bool Remove(IUnique<IInstant> item)
        {
            return Proxies.Remove(item);
        }

        public bool TryRemove(object key)
        {
            return Proxies.TryRemove(key);
        }

        public void RemoveAt(int index)
        {
            Proxies.RemoveAt(index);
        }

        public void Renew(IEnumerable<IInstant> items)
        {
            Proxies.Renew(items);
        }

        public void Renew(IList<IInstant> items)
        {
            Proxies.Renew(items);
        }

        public void Renew(IList<ISeriesItem<IInstant>> items)
        {
            Proxies.Renew(items);
        }

        public void Renew(IEnumerable<ISeriesItem<IInstant>> items)
        {
            Proxies.Renew(items);
        }

        public IInstant[] ToArray()
        {
            return Proxies.ToArray();
        }

        public void CopyTo(IInstant[] array, int arrayIndex)
        {
            Proxies.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            Proxies.CopyTo(array, arrayIndex);
        }

        public void CopyTo(ISeriesItem<IInstant>[] array, int destIndex)
        {
            Proxies.CopyTo(array, destIndex);
        }

        public ISeriesItem<IInstant> NewItem(IInstant value)
        {
            return Proxies.NewItem(value);
        }

        public ISeriesItem<IInstant> NewItem(ISeriesItem<IInstant> value)
        {
            return Proxies.NewItem(value);
        }

        public ISeriesItem<IInstant> NewItem(object key, IInstant value)
        {
            return Proxies.NewItem(key, value);
        }

        public ISeriesItem<IInstant> NewItem(ulong key, IInstant value)
        {
            return Proxies.NewItem(key, value);
        }

        public int IndexOf(IInstant item)
        {
            return Proxies.IndexOf(item);
        }

        public void Insert(int index, IInstant item)
        {
            Series.Add(item);
            Proxies.Insert(index, item);
        }

        public IEnumerator<IInstant> GetEnumerator()
        {
            return ((IEnumerable<IInstant>)Proxies).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Proxies).GetEnumerator();
        }

        public byte[] GetBytes()
        {
            return Proxies.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return Proxies.GetKeyBytes();
        }

        public bool Equals(IUnique other)
        {
            return Proxies.Equals(other);
        }

        public int CompareTo(IUnique other)
        {
            return Proxies.CompareTo(other);
        }

        public IEnumerable<ISeriesItem<IInstant>> AsItems()
        {
            return Proxies.AsItems();
        }

        public IEnumerable<IInstant> AsValues()
        {
            return Proxies.AsValues();
        }

        public bool ContainsKey(ulong key)
        {
            return Proxies.ContainsKey(key);
        }

        public IInstant Get(ulong key)
        {
            return Proxies.Get(key);
        }

        public bool TryGet(ulong key, out IInstant output)
        {
            return Proxies.TryGet(key, out output);
        }

        public ISeriesItem<IInstant> GetItem(ulong key)
        {
            return Proxies.GetItem(key);
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }

        public int ItemsCount => Proxies.Count;

        public int Count => Proxies.Count;

        public int MinCount
        {
            get => Proxies.MinCount;
            set => Proxies.MinCount = value;
        }

        public IRubrics Rubrics
        {
            get => Proxies.Rubrics;
            set => Proxies.Rubrics = value;
        }

        public IRubrics KeyRubrics
        {
            get => Proxies.KeyRubrics;
            set => Proxies.KeyRubrics = value;
        }

        public Type FigureType
        {
            get => Series.FigureType;
            set => Series.FigureType = value;
        }

        public int FigureSize
        {
            get => Series.FigureSize;
            set => Series.FigureSize = value;
        }

        public ISeriesItem<IInstant> First => Proxies.First;

        public ISeriesItem<IInstant> Last => Proxies.Last;

        public bool IsSynchronized
        {
            get => Proxies.IsSynchronized;
            set => Proxies.IsSynchronized = value;
        }
        public object SyncRoot
        {
            get => Proxies.SyncRoot;
            set => Proxies.SyncRoot = value;
        }

        public bool IsReadOnly => Proxies.IsReadOnly;

        bool ICollection.IsSynchronized => Proxies.IsSynchronized;

        object ICollection.SyncRoot => Proxies.SyncRoot;

        public object[] ValueArray
        {
            get => Proxies.ValueArray;
            set => Proxies.ValueArray = value;
        }

        public Uscn Code
        {
            get => Proxies.Code;
            set => Proxies.Code = value;
        }

        public ulong Key
        {
            get => Proxies.Key;
            set => Proxies.Key = value;
        }

        public Type Type
        {
            get => Series.Type;
            set => Series.Type = value;
        }    

        public InstantSeriesAggregate Aggregate
        {
            get => Proxies.Aggregate;
            set => Proxies.Aggregate = value;
        }

        public IInstant Total
        {
            get => Proxies.Total;
            set => Proxies.Total = value;
        }

        public ISeries<IInstantSeriesMath> Computations
        {
            get => Series.Computations;
            set => Series.Computations = value;
        }
        public ulong TypeKey
        {
            get => Proxies.TypeKey;
            set => Proxies.TypeKey = value;
        }

        public IInstant this[int index]
        {
            get => Proxies[index];
            set => Proxies[index] = value;
        }

        object IInstant.this[int fieldId]
        {
            get => Proxies[fieldId];
            set => Proxies[fieldId] = (IInstant)value;
        }

        public object this[string propertyName]
        {
            get => Proxies[propertyName];
            set => Proxies[propertyName] = value;
        }

        public IInstant this[object key]
        {
            get => Proxies[key];
            set => Proxies[key] = value;
        }

        object IFindableSeries.this[object key]
        {
            get => Proxies[key];
            set => Proxies[key] = (IInstant)value;
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Proxies.Dispose();
                }
                Proxies = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool TryGet(IUnique key, out ISeriesItem<IInstant> output)
        {
            return Proxies.TryGet(key, out output);
        }

        public bool TryGet(IUnique<IInstant> key, out ISeriesItem<IInstant> output)
        {
            return Proxies.TryGet(key, out output);
        }

        public ISeriesItem<IInstant> GetItem(IUnique key)
        {
            return Proxies.GetItem(key);
        }

        public ISeriesItem<IInstant> GetItem(IUnique<IInstant> key)
        {
            return Proxies.GetItem(key);
        }

        public ISeriesItem<IInstant> Set(object key, IInstant value)
        {
            return Proxies.Set(key, value);
        }

        public ISeriesItem<IInstant> Set(ulong key, IInstant value)
        {
            return Proxies.Set(key, value);
        }

        public ISeriesItem<IInstant> Set(IUnique key, IInstant value)
        {
            return Proxies.Set(key, value);
        }

        public ISeriesItem<IInstant> Set(IUnique<IInstant> key, IInstant value)
        {
            return Proxies.Set(key, value);
        }

        public ISeriesItem<IInstant> Set(IInstant value)
        {
            return Proxies.Set(value);
        }

        public ISeriesItem<IInstant> Set(IUnique<IInstant> value)
        {
            return Proxies.Set(value);
        }

        public ISeriesItem<IInstant> Set(ISeriesItem<IInstant> value)
        {
            return Proxies.Set(value);
        }

        public int Set(IEnumerable<IInstant> values)
        {
            return Proxies.Set(values);
        }

        public int Set(IList<IInstant> values)
        {
            return Proxies.Set(values);
        }

        public int Set(IEnumerable<ISeriesItem<IInstant>> values)
        {
            return Proxies.Set(values);
        }

        public int Set(IEnumerable<IUnique<IInstant>> values)
        {
            return Proxies.Set(values);
        }

        public ISeriesItem<IInstant> EnsureGet(object key, Func<ulong, IInstant> sureaction)
        {
            return Proxies.EnsureGet(key, sureaction);
        }

        public ISeriesItem<IInstant> EnsureGet(ulong key, Func<ulong, IInstant> sureaction)
        {
            return Proxies.EnsureGet((object)key, sureaction);
        }

        public ISeriesItem<IInstant> EnsureGet(IUnique key, Func<ulong, IInstant> sureaction)
        {
            return Proxies.EnsureGet((object)key, sureaction);
        }

        public ISeriesItem<IInstant> EnsureGet(IUnique<IInstant> key, Func<ulong, IInstant> sureaction)
        {
            return Proxies.EnsureGet((object)key, sureaction);
        }

        public bool TryPick(int skip, out IInstant output)
        {
            return Proxies.TryPick(skip, out output);
        }

        public void CopyTo(IUnique<IInstant>[] array, int arrayIndex)
        {
            Proxies.CopyTo(array, arrayIndex);
        }

        public void Resize(int size)
        {
            Proxies.Resize(size);
        }

        public bool Remove(object key, IInstant item)
        {
            throw new NotImplementedException();
        }
    }
}
