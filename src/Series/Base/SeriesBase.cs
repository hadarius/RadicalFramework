
namespace Radical.Series.Base
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Radical.Uniques;
    using Enumerators;

    public abstract class SeriesBase<V> : UniqueKey, ISeries<V>, ISet<V>, IAsyncDisposable
    {

        internal const float RESIZING_VECTOR = 1.766F;
        internal const float CONFLICTS_PERCENT_LIMIT = 0.222F;
        internal const float REMOVED_PERCENT_LIMIT = 0.15F;

        protected Uscn serialcode;
        protected ISeriesItem<V> first, last;
        protected ISeriesItem<V>[] table;
        protected readonly int minSize;
        protected int count, conflicts, removed, size, mincount;
        protected uint maxId;

        private int nextSize()
        {
            return (((int)(size * RESIZING_VECTOR)) | 3);
        }

        private int previousSize()
        {

            return (int)(size * (1 - REMOVED_PERCENT_LIMIT)) | 3;
        }

        protected void countIncrement()
        {
            if ((++count + 3) > size)
                Rehash(nextSize());
        }
        protected void conflictIncrement()
        {
            countIncrement();
            if (++conflicts > (size * CONFLICTS_PERCENT_LIMIT))
                Rehash(nextSize());
        }
        protected void removedIncrement()
        {
            --count;
            if (++removed > ((size * REMOVED_PERCENT_LIMIT) - 1))
            {
                if (size < (size * 0.5))
                    Rehash(previousSize());
                else
                    Rehash(size);
            }
        }
        protected void removedDecrement()
        {
            ++count;
            --removed;
        }

        protected SeriesBase(int capacity = 17, HashBits bits = HashBits.bit64) : base(bits)
        {
            size = capacity;
            minSize = capacity;
            maxId = (uint)(capacity - 1);
            table = EmptyTable(capacity);
            first = EmptyItem();
            last = first;
            ValueEquals = getValueComparer();
            serialcode = new Uscn(typeof(V).UniqueKey64());
        }
        protected SeriesBase(IList<ISeriesItem<V>> collection, int capacity = 17, HashBits bits = HashBits.bit64) : this(capacity > collection.Count ? capacity : collection.Count, bits)
        {
            if (collection != null)
                this.Add(collection);
        }
        protected SeriesBase(IList<IUnique<V>> collection, int capacity = 17, HashBits bits = HashBits.bit64) : this(capacity > collection.Count ? capacity : collection.Count, bits)
        {
            if (collection != null)
                foreach (var c in collection)
                    this.Add(c);
        }
        protected SeriesBase(IEnumerable<ISeriesItem<V>> collection, int capacity = 17, HashBits bits = HashBits.bit64) : this(capacity, bits)
        {
            if (collection != null)
                this.Add(collection);
        }
        protected SeriesBase(IEnumerable<IUnique<V>> collection, int capacity = 17, HashBits bits = HashBits.bit64) : this(capacity, bits)
        {
            if (collection != null)
                foreach (var c in collection)
                    this.Add(c);
        }

        public virtual ISeriesItem<V> First
        { get { return first; } }
        public virtual ISeriesItem<V> Last
        { get { return last; } }

        public virtual int Size => size;
        public virtual int Count => count;
        public virtual int MinCount
        {
            get => mincount;
            set => mincount = value;
        }
        public virtual bool IsReadOnly { get; set; }
        public virtual bool IsSynchronized { get; set; }
        public virtual bool IsRepeatable { get => false; }
        public virtual object SyncRoot { get; set; }
        public virtual Func<V, V, bool> ValueEquals { get; }

        public virtual V this[int index]
        {
            get => GetItem(index).Value;
            set => GetItem(index).Value = value;
        }
        protected V this[ulong hashkey]
        {
            get => InnerGet(hashkey);
            set => InnerPut(hashkey, value);
        }
        public virtual V this[object key]
        {
            get => InnerGet(unique.Key(key));
            set => InnerPut(unique.Key(key), value);
        }
        public virtual V this[IUnique key]
        {
            get => InnerGet(unique.Key(key));
            set => InnerPut(unique.Key(key), value);
        }
        object IFindableSeries.this[object key]
        {
            get => InnerGet(unique.Key(key));
            set => InnerPut(unique.Key(key), (V)value);
        }

        protected virtual V InnerGet(ulong key)
        {
            ISeriesItem<V> mem = table[getPosition(key)];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (!mem.Removed)
                        return mem.Value;
                    return default(V);
                }
                mem = mem.Extended;
            }

            return default(V);
        }
        public virtual V Get(ulong key)
        {
            return InnerGet(key);
        }
        public virtual V Get(object key)
        {
            return InnerGet(unique.Key(key));
        }
        public virtual V Get(IUnique key)
        {
            return InnerGet(unique.Key(key));
        }
        public virtual V Get(IUnique<V> key)
        {
            return InnerGet(key.CompactKey());
        }

        protected virtual bool InnerTryGet(ulong key, out ISeriesItem<V> output)
        {
            output = null;

            ISeriesItem<V> mem = table[getPosition(key)];
            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (!mem.Removed)
                    {
                        output = mem;
                        return true;
                    }
                    return false;
                }
                mem = mem.Extended;
            }
            return false;
        }
        public virtual bool TryGet(ulong key, out ISeriesItem<V> output)
        {
            return InnerTryGet(key, out output);
        }
        public virtual bool TryGet(object key, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key), out output);
        }
        public virtual bool TryGet(IUnique key, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key), out output);
        }
        public virtual bool TryGet(IUnique<V> key, out ISeriesItem<V> output)
        {
            return InnerTryGet(key.CompactKey(), out output);
        }

        public virtual bool TryGet(object key, out V output)
        {
            output = default(V);
            if (InnerTryGet(unique.Key(key), out ISeriesItem<V> item))
            {
                output = item.Value;
                return true;
            }
            return false;
        }
        public virtual bool TryGet(ulong key, out V output)
        {
            if (InnerTryGet(key, out ISeriesItem<V> item))
            {
                output = item.Value;
                return true;
            }
            output = default(V);
            return false;
        }
        public virtual bool TryGet(IUnique key, out V output)
        {
            output = default;
            if (!InnerTryGet(unique.Key(key), out ISeriesItem<V> _output))
                return false;
            output = _output.Value;
            return true;
        }
        public virtual bool TryGet(IUnique<V> key, out V output)
        {
            output = default;
            if (!InnerTryGet(key.CompactKey(), out ISeriesItem<V> _output))
                return false;
            output = _output.Value;
            return true;
        }

        protected virtual ISeriesItem<V> InnerGetItem(ulong key)
        {
            ISeriesItem<V> mem = table[getPosition(key)];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (!mem.Removed)
                        return mem;
                    return null;
                }
                mem = mem.Extended;
            }

            return mem;
        }
        public virtual ISeriesItem<V> GetItem(ulong key)
        {
            return InnerGetItem(key);
        }
        public virtual ISeriesItem<V> GetItem(object key)
        {
            return InnerGetItem(unique.Key(key));
        }
        public virtual ISeriesItem<V> GetItem(IUnique key)
        {
            return InnerGetItem(unique.Key(key, key.TypeKey));
        }
        public virtual ISeriesItem<V> GetItem(IUnique<V> key)
        {
            return InnerGetItem(key.CompactKey());
        }
        public abstract ISeriesItem<V> GetItem(int index);

        public virtual ISeriesItem<V> EnsureGet(object key, Func<ulong, V> sureaction)
        {
            ulong _key = unique.Key(key);
            return (!TryGet(_key, out ISeriesItem<V> item)) ?
                Put(key, sureaction.Invoke(_key)) : item;
        }
        public virtual ISeriesItem<V> EnsureGet(ulong key, Func<ulong, V> sureaction)
        {
            return (!TryGet(key, out ISeriesItem<V> item)) ?
                Put(key, sureaction.Invoke(key)) : item;
        }
        public virtual ISeriesItem<V> EnsureGet(IUnique key, Func<ulong, V> sureaction)
        {
            ulong _key = unique.Key(key);
            return (!TryGet(_key, out ISeriesItem<V> item)) ?
                Put(key, sureaction.Invoke(_key)) : item;
        }
        public virtual ISeriesItem<V> EnsureGet(IUnique<V> key, Func<ulong, V> sureaction)
        {
            ulong _key = key.CompactKey();
            return (!TryGet(_key, out ISeriesItem<V> item)) ?
                Put(key, sureaction.Invoke(_key)) : item;
        }

        protected virtual ISeriesItem<V> InnerSet(ulong key, V value)
        {
            var item = InnerGetItem(key);
            if (item != null) item.Value = value;
            return item;
        }
        protected virtual ISeriesItem<V> InnerSet(ISeriesItem<V> value)
        {
            var item = InnerGetItem(value.Key);
            if (item != null) item.Value = value.Value;
            return item;
        }
        protected virtual ISeriesItem<V> InnerSet(V value)
        {
            var item = InnerGetItem(unique.Key(value));
            if (item != null) item.Value = value;
            return item;
        }
        public virtual ISeriesItem<V> Set(object key, V value)
        {
            return InnerSet(unique.Key(key), value);
        }
        public virtual ISeriesItem<V> Set(ulong key, V value)
        {
            return InnerSet(key, value);
        }
        public virtual ISeriesItem<V> Set(IUnique key, V value)
        {
            return InnerSet(unique.Key(key), value);
        }
        public virtual ISeriesItem<V> Set(IUnique<V> key, V value)
        {
            return InnerSet(key.CompactKey(), value);
        }
        public virtual ISeriesItem<V> Set(V value)
        {
            return InnerSet(value);
        }
        public virtual ISeriesItem<V> Set(IUnique<V> value)
        {
            return InnerSet(value.CompactKey(), value.UniqueObject);
        }
        public virtual ISeriesItem<V> Set(ISeriesItem<V> value)
        {
            return InnerSet(value);
        }
        public virtual int Set(IEnumerable<V> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (Set(value) != null)
                    count++;
            }

            return count;
        }
        public virtual int Set(IList<V> values)
        {
            int i = 0, c = values.Count;
            while (--c > -1)
            {
                if (Set(values[c]) != null)
                    i++;
            }
            return i;
        }
        public virtual int Set(IEnumerable<ISeriesItem<V>> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (InnerSet(value) != null)
                    count++;
            }

            return count;
        }
        public virtual int Set(IEnumerable<IUnique<V>> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (Set(value) != null)
                    count++;
            }

            return count;
        }

        protected abstract ISeriesItem<V> InnerPut(ulong key, V value);
        protected abstract ISeriesItem<V> InnerPut(V value);
        protected abstract ISeriesItem<V> InnerPut(ISeriesItem<V> value);
        public virtual ISeriesItem<V> Put(ulong key, object value)
        {
            return InnerPut(key, (V)value);
        }
        public virtual ISeriesItem<V> Put(ulong key, V value)
        {
            return InnerPut(key, value);
        }
        public virtual ISeriesItem<V> Put(object key, V value)
        {
            return InnerPut(unique.Key(key), value);
        }
        public virtual ISeriesItem<V> Put(object key, object value)
        {
            if (value is V)
                return InnerPut(unique.Key(key), (V)value);
            return null;
        }
        public virtual ISeriesItem<V> Put(ISeriesItem<V> item)
        {
            return InnerPut(item);
        }
        public virtual void Put(IList<ISeriesItem<V>> items)
        {
            int i = 0, c = items.Count;
            while (i < c)
                InnerPut(items[i++]);
        }
        public virtual void Put(IEnumerable<ISeriesItem<V>> items)
        {
            foreach (ISeriesItem<V> item in items)
                InnerPut(item);
        }
        public virtual ISeriesItem<V> Put(V value)
        {
            return InnerPut(value);
        }
        public virtual void Put(object value)
        {
            if (value is IUnique<V>)
                Put((IUnique<V>)value);
            if (value is V)
                Put((V)value);
            else if (value is ISeriesItem<V>)
                Put((ISeriesItem<V>)value);
        }
        public virtual void Put(IList<V> items)
        {
            int i = 0, c = items.Count;
            while (i < c)
                InnerPut(items[i++]);
        }
        public virtual void Put(IEnumerable<V> items)
        {
            foreach (V item in items)
                InnerPut(item);
        }
        public virtual ISeriesItem<V> Put(IUnique<V> value)
        {
            return InnerPut(unique.Key(value), value.UniqueObject);
        }
        public virtual void Put(IList<IUnique<V>> value)
        {
            int i = 0, c = value.Count;
            while (i < c)
                Put(value[i++]);
        }
        public virtual void Put(IEnumerable<IUnique<V>> value)
        {
            foreach (IUnique<V> item in value)
            {
                Put(item);
            }
        }

        protected abstract bool InnerAdd(ulong key, V value);
        protected abstract bool InnerAdd(V value);
        protected abstract bool InnerAdd(ISeriesItem<V> value);
        public virtual bool Add(ulong key, object value)
        {
            return InnerAdd(key, (V)value);
        }
        public virtual bool Add(ulong key, V value)
        {
            return InnerAdd(key, value);
        }
        public virtual bool Add(object key, V value)
        {
            return InnerAdd(unique.Key(key), value);
        }
        public virtual void Add(ISeriesItem<V> item)
        {
            InnerAdd(item);
        }
        public virtual void Add(IList<ISeriesItem<V>> itemList)
        {
            int i = 0, c = itemList.Count;
            while (i < c)
                InnerAdd(itemList[i++]);
        }
        public virtual void Add(IEnumerable<ISeriesItem<V>> itemTable)
        {
            foreach (ISeriesItem<V> item in itemTable)
                Add(item);
        }
        public virtual void Add(V value)
        {
            InnerAdd(value);
        }
        bool ISet<V>.Add(V value)
        {
            return InnerAdd(value);
        }
        public virtual void Add(IList<V> items)
        {
            int i = 0, c = items.Count;
            while (i < c)
                InnerAdd(items[i++]);
        }
        public virtual void Add(IEnumerable<V> items)
        {
            foreach (V item in items)
                Add(item);
        }
        public virtual void Add(IUnique<V> value)
        {
            InnerAdd(unique.Key(value), value.UniqueObject);
        }
        public virtual void Add(IList<IUnique<V>> value)
        {
            int i = 0, c = value.Count;
            while (i < c)
                Add(value[i++]);
        }
        public virtual void Add(IEnumerable<IUnique<V>> value)
        {
            foreach (IUnique<V> item in value)
            {
                Add(item);
            }
        }
        public virtual bool TryAdd(V value)
        {
            return InnerAdd(value);
        }
        public virtual bool TryAdd(object key, V value)
        {
            return InnerAdd(unique.Key(key), value);
        }

        public virtual ISeriesItem<V> New()
        {
            ISeriesItem<V> newItem = NewItem(Unique.NewKey, default(V));
            if (InnerAdd(newItem))
                return newItem;
            return null;
        }
        public virtual ISeriesItem<V> New(ulong key)
        {
            ISeriesItem<V> newItem = NewItem(key, default(V));
            if (InnerAdd(newItem))
                return newItem;
            return null;
        }
        public virtual ISeriesItem<V> New(object key)
        {
            ISeriesItem<V> newItem = NewItem(unique.Key(key), default(V));
            if (InnerAdd(newItem))
                return newItem;
            return null;
        }

        protected abstract void InnerInsert(int index, ISeriesItem<V> item);
        public virtual void Insert(int index, ISeriesItem<V> seriesItem)
        {

            ulong key = seriesItem.Key;
            ulong pos = getPosition(key);

            ISeriesItem<V> item = table[pos];

            if (item == null)
            {
                item = NewItem(item);
                table[pos] = item;
                InnerInsert(index, item);
                countIncrement();
                return;
            }

            for (; ; )
            {

                if (item.Equals(key))
                {

                    if (item.Removed)
                    {
                        var newitem = NewItem(item);
                        item.Extended = newitem;
                        InnerInsert(index, newitem);
                        conflictIncrement();
                        return;
                    }
                    throw new Exception("SeriesItem exist");

                }

                if (item.Extended == null)
                {
                    var newitem = NewItem(item);
                    item.Extended = newitem;
                    InnerInsert(index, newitem);
                    conflictIncrement();
                    return;
                }
                item = item.Extended;
            }
        }
        public virtual void Insert(int index, V value)
        {

            ulong key = unique.Key(value);
            ulong pos = getPosition(key);

            ISeriesItem<V> item = table[pos];

            if (item == null)
            {
                item = NewItem(item);
                table[pos] = item;
                InnerInsert(index, item);
                countIncrement();
                return;
            }

            for (; ; )
            {

                if (item.Equals(key))
                {

                    if (item.Removed)
                    {
                        var newitem = NewItem(item);
                        item.Extended = newitem;
                        InnerInsert(index, newitem);
                        conflictIncrement();
                        return;
                    }
                    throw new Exception("SeriesItem exist");

                }

                if (item.Extended == null)
                {
                    var newitem = NewItem(item);
                    item.Extended = newitem;
                    InnerInsert(index, newitem);
                    conflictIncrement();
                    return;
                }
                item = item.Extended;
            }
        }

        public virtual bool Enqueue(V value)
        {
            return InnerAdd(value);
        }
        public virtual bool Enqueue(object key, V value)
        {
            return InnerAdd(unique.Key(key), value);
        }
        public virtual void Enqueue(ISeriesItem<V> item)
        {
            InnerAdd(item);
        }

        public virtual void SetMinCount(int minCount)
        {
            mincount = minCount;
        }

        public virtual V Dequeue()
        {
            var item = Next(first);
            if (item != null)
            {
                item.Removed = true;
                removedIncrement();
                first = item;
                return item.Value;
            }
            return default(V);
        }
        public virtual bool TryDequeue(out V output)
        {
            output = default(V);
            if (count < mincount)
                return false;

            var item = Next(first);
            if (item != null)
            {
                item.Removed = true;
                removedIncrement();
                first = item;
                output = item.Value;
                return true;
            }
            return false;
        }
        public virtual bool TryDequeue(out ISeriesItem<V> output)
        {
            output = null;
            if (count < mincount)
                return false;

            output = Next(first);
            if (output != null)
            {
                output.Removed = true;
                removedIncrement();
                first = output;
                return true;
            }
            return false;
        }

        public virtual bool TryPick(int skip, out V output)
        {
            output = this.Skip(skip).FirstOrDefault();
            if (output != null)
            {
                return true;
            }
            return false;
        }

        public virtual bool TryTake(out V output)
        {
            return TryDequeue(out output);
        }

        protected virtual void renewClear(int capacity)
        {
            if (capacity != size || count > 0)
            {
                size = capacity;
                maxId = (uint)(capacity - 1);
                conflicts = 0;
                removed = 0;
                count = 0;
                table = EmptyTable(size);
                first = EmptyItem();
                last = first;
            }
        }

        public virtual void Renew(IEnumerable<V> items)
        {
            renewClear(minSize);
            Put(items);
        }
        public virtual void Renew(IList<V> items)
        {
            int capacity = items.Count;
            capacity += (int)(capacity * CONFLICTS_PERCENT_LIMIT);
            renewClear(capacity);
            Put(items);
        }
        public virtual void Renew(IList<ISeriesItem<V>> items)
        {
            int capacity = items.Count;
            capacity += (int)(capacity * CONFLICTS_PERCENT_LIMIT);
            renewClear(capacity);
            Put(items);
        }
        public virtual void Renew(IEnumerable<ISeriesItem<V>> items)
        {
            renewClear(minSize);
            Put(items);
        }

        protected bool InnerContainsKey(ulong key)
        {
            ISeriesItem<V> mem = table[getPosition(key)];

            while (mem != null)
            {
                if (!mem.Removed && mem.Equals(key))
                {
                    return true;
                }
                mem = mem.Extended;
            }

            return false;
        }
        public virtual bool ContainsKey(object key)
        {
            return InnerContainsKey(unique.Key(key));
        }
        public virtual bool ContainsKey(ulong key)
        {
            return InnerContainsKey(key);
        }
        public virtual bool ContainsKey(IUnique key)
        {
            return InnerContainsKey(unique.Key(key));
        }

        public virtual bool Contains(ISeriesItem<V> item)
        {
            return InnerContainsKey(item.Key);
        }
        public virtual bool Contains(IUnique<V> item)
        {
            return InnerContainsKey(unique.Key(item));
        }
        public virtual bool Contains(V item)
        {
            return InnerContainsKey(unique.Key(item));
        }
        public virtual bool Contains(ulong key, V item)
        {
            return InnerContainsKey(key);
        }

        protected virtual Func<V, V, bool> getValueComparer()
        {
            if (typeof(V).IsValueType)
                return (o1, o2) => o1.Equals(o2);
            return (o1, o2) => ReferenceEquals(o1, o2);
        }

        protected virtual V InnerRemove(ulong key)
        {
            ISeriesItem<V> mem = table[getPosition(key)];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (mem.Removed)
                        return default(V);

                    mem.Removed = true;
                    removedIncrement();
                    return mem.Value;
                }

                mem = mem.Extended;
            }
            return default(V);
        }
        protected virtual V InnerRemove(ulong key, V item)
        {
            ISeriesItem<V> mem = table[getPosition(key)];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (mem.Removed)
                        return default(V);

                    if (ValueEquals(mem.Value, item))
                    {
                        mem.Removed = true;
                        removedIncrement();
                        return mem.Value;
                    }
                    return default(V);
                }
                mem = mem.Extended;
            }
            return default(V);
        }
        public virtual bool Remove(V item)
        {
            return InnerRemove(unique.Key(item)).Equals(default(V)) ? false : true;
        }
        public virtual V Remove(object key)
        {
            return InnerRemove(unique.Key(key));
        }
        public virtual bool Remove(ISeriesItem<V> item)
        {
            return InnerRemove(item.Key).Equals(default(V)) ? false : true;
        }
        public virtual bool Remove(IUnique<V> item)
        {
            return InnerRemove(unique.Key(item)).Equals(default(V)) ? false : true;
        }
        public virtual bool TryRemove(object key)
        {
            V result = InnerRemove(unique.Key(key));
            if (result != null &&
                !result.Equals(default(V)))
                return true;
            return false;
        }
        public virtual void RemoveAt(int index)
        {
            InnerRemove(GetItem(index).Key);
        }
        public virtual bool Remove(object key, V item)
        {
            return InnerRemove(unique.Key(key), item).Equals(default(V)) ? false : true;
        }

        public virtual void Clear()
        {
            size = minSize;
            maxId = (uint)(minSize - 1);
            conflicts = 0;
            removed = 0;
            count = 0;
            table = EmptyTable(size);
            first = EmptyItem();
            last = first;
        }

        public virtual void Flush()
        {
            conflicts = 0;
            removed = 0;
            count = 0;
            table = null;
            table = EmptyTable(size);
            first = EmptyItem();
            last = first;
        }

        public virtual void CopyTo(ISeriesItem<V>[] array, int index)
        {
            int c = count, i = index, l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (ISeriesItem<V> ves in this.Take(c))
                    array[i++] = ves;
            }
            else
                foreach (ISeriesItem<V> ves in this)
                    array[i++] = ves;
        }
        public virtual void CopyTo(IUnique<V>[] array, int arrayIndex)
        {
            int c = count, i = arrayIndex, l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (ISeriesItem<V> ves in this.Take(c))
                    array[i++] = ves;
            }
            else
                foreach (ISeriesItem<V> ves in this)
                    array[i++] = ves;
        }
        public virtual void CopyTo(Array array, int index)
        {
            int c = count, i = index, l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (V ves in this.AsValues().Take(c))
                    array.SetValue(ves, i++);
            }
            else
                foreach (V ves in this.AsValues())
                    array.SetValue(ves, i++);
        }
        public virtual void CopyTo(V[] array, int index)
        {
            int c = count, i = index, l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (V ves in this.AsValues().Take(c))
                    array[i++] = ves;
            }
            else
                foreach (V ves in this.AsValues())
                    array[i++] = ves;
        }

        public virtual V[] ToArray()
        {
            return this.AsValues().ToArray();
        }
        public virtual object[] ToObjectArray()
        {
            return AsValues().Cast<object>().ToArray();
        }

        public virtual ISeriesItem<V> Next(ISeriesItem<V> item)
        {
            ISeriesItem<V> _item = item.Next;
            if (_item != null)
            {
                if (!_item.Removed)
                    return _item;
                return Next(_item);
            }
            return null;
        }

        public virtual void Resize(int size)
        {
            Rehash(size);
        }

        public abstract ISeriesItem<V> EmptyItem();

        public abstract ISeriesItem<V> NewItem(ulong key, V value);
        public abstract ISeriesItem<V> NewItem(object key, V value);
        public abstract ISeriesItem<V> NewItem(ISeriesItem<V> item);
        public abstract ISeriesItem<V> NewItem(V item);

        public abstract ISeriesItem<V>[] EmptyTable(int size);

        public virtual int IndexOf(ISeriesItem<V> item)
        {
            if (item != null)
                return item.Index;
            return -1;
        }
        public virtual int IndexOf(V value)
        {
            var item = GetItem(value);
            if (item != null)
                return item.Index;
            return -1;
        }
        protected virtual int IndexOf(ulong key, V value)
        {
            var item = GetItem(key);
            if (item != null && ValueEquals(item.Value, value))
                return item.Index;
            return -1;
        }

        public virtual IEnumerable<V> AsValues()
        {
            return this;
        }
        public virtual IEnumerable<ISeriesItem<V>> AsItems()
        {
            foreach (ISeriesItem<V> item in this)
            {
                yield return item;
            }
        }
        public virtual IEnumerator<ISeriesItem<V>> GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }
        public virtual IAsyncEnumerator<V> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new ItemSeriesAsync<V>(this);
        }
        public virtual IEnumerator<ulong> GetKeyEnumerator()
        {
            return new SeriesItemUniqueKeyEnumerator<V>(this);
        }

        IEnumerator<V> IEnumerable<V>.GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }

        protected ulong getPosition(ulong key)
        {

            return (key % maxId);
        }
        protected static ulong getPosition(ulong key, uint tableMaxId)
        {
            return (key % tableMaxId);
        }

        protected virtual void Rehash(int newSize)
        {
            int finish = count;
            int newsize = newSize;
            uint newMaxId = (uint)(newsize - 1);
            ISeriesItem<V>[] newitemTable = EmptyTable(newsize);
            ISeriesItem<V> item = first;
            item = item.Next;
            if (removed > 0)
            {
                rehashAndReindex(item, newitemTable, newMaxId);
            }
            else
            {
                rehash(item, newitemTable, newMaxId);
            }

            table = newitemTable;
            maxId = newMaxId;
            size = newsize;
        }

        private void rehashAndReindex(ISeriesItem<V> item, ISeriesItem<V>[] newItemTable, uint newMaxId)
        {
            int _conflicts = 0;
            uint _newMaxId = newMaxId;
            ISeriesItem<V>[] _newItemTable = newItemTable;
            ISeriesItem<V> _firstitem = EmptyItem();
            ISeriesItem<V> _lastitem = _firstitem;
            do
            {
                if (!item.Removed)
                {
                    ulong pos = getPosition(item.Key, _newMaxId);

                    ISeriesItem<V> ex_item = _newItemTable[pos];

                    if (ex_item == null)
                    {
                        item.Extended = null;
                        _newItemTable[pos] = _lastitem = _lastitem.Next = item;
                    }
                    else
                    {
                        for (; ; )
                        {
                            if (ex_item.Extended == null)
                            {
                                item.Extended = null; ;
                                _lastitem = _lastitem.Next = ex_item.Extended = item;
                                _conflicts++;
                                break;
                            }
                            else
                                ex_item = ex_item.Extended;
                        }
                    }
                }

                item = item.Next;

            } while (item != null);

            conflicts = _conflicts;
            removed = 0;
            first = _firstitem;
            last = _lastitem;
        }

        private void rehash(ISeriesItem<V> item, ISeriesItem<V>[] newItemTable, uint newMaxId)
        {
            int _conflicts = 0;
            uint _newMaxId = newMaxId;
            ISeriesItem<V>[] _newItemTable = newItemTable;
            do
            {
                if (!item.Removed)
                {
                    ulong pos = getPosition(item.Key, _newMaxId);

                    ISeriesItem<V> ex_item = _newItemTable[pos];

                    if (ex_item == null)
                    {
                        item.Extended = null;
                        _newItemTable[pos] = item;
                    }
                    else
                    {
                        for (; ; )
                        {
                            if (ex_item.Extended == null)
                            {
                                item.Extended = null;
                                ex_item.Extended = item;
                                _conflicts++;
                                break;
                            }
                            else
                                ex_item = ex_item.Extended;
                        }
                    }
                }

                item = item.Next;

            } while (item != null);
            conflicts = _conflicts;
        }

        protected bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    first = null;
                    last = null;
                    table = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsyncCore()
        {
            await new ValueTask(Task.Run(() =>
            {
                first = null;
                last = null;
                table = null;

            })).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(false);

            GC.SuppressFinalize(this);
        }

        public virtual bool Equals(IUnique? other)
        {
            return serialcode.Equals(other);
        }

        public virtual int CompareTo(IUnique? other)
        {
            return serialcode.CompareTo(other);
        }

        public IUnique Empty => Usid.Empty;

        public virtual ulong Key
        {
            get => serialcode.Key;
            set => serialcode.Key = value;
        }

        public virtual ulong TypeKey
        {
            get => serialcode.TypeKey;
            set => serialcode.TypeKey = value;
        }

        public virtual byte[] GetBytes()
        {
            return serialcode.GetBytes();
        }

        public virtual byte[] GetKeyBytes()
        {
            return serialcode.GetKeyBytes();
        }

        public virtual void ExceptWith(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual void IntersectWith(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool IsProperSubsetOf(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool IsProperSupersetOf(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool IsSubsetOf(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool IsSupersetOf(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool Overlaps(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual bool SetEquals(IEnumerable<V> other)
        {
            throw new NotImplementedException();
        }
        public virtual void SymmetricExceptWith(IEnumerable<V> other)
        {
            other.ForOnly(e => Contains(e), (e) => Remove(e));
        }
        public virtual void UnionWith(IEnumerable<V> other)
        {
            this.Add(other);
        }
    }
}
