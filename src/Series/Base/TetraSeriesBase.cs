﻿using System.Linq;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Radical.Series.Base
{
    using Tetra;
    using Uniques;
    using Enumerators;

    public abstract class TetraSeriesBase<V>
        : UniqueKey,
            ICollection<V>,
            IList<V>,
            ISeries<V>,
            ICollection<ISeriesItem<V>>,
            IList<ISeriesItem<V>>,
            IProducerConsumerCollection<V>,
            IDisposable,
            ICollection<IUnique<V>>
    {
        static protected readonly float CONFLICTS_PERCENT_LIMIT = 0.25f;
        static protected readonly float REMOVED_PERCENT_LIMIT = 0.15f;

        protected Uscn code;
        protected ISeriesItem<V> first,
            last;
        protected TetraTable<V> table;
        protected TetraSize tsize;
        protected TetraCount tcount;
        protected int count,
            conflicts,
            removed,
            size,
            minSize;

        protected void countIncrement(uint tid)
        {
            count++;
            if ((tcount.Increment(tid) + 3) > size)
                Rehash(tsize.NextSize(tid), tid);
        }

        protected void conflictIncrement(uint tid)
        {
            countIncrement(tid);
            if (++conflicts > (size * CONFLICTS_PERCENT_LIMIT))
                Rehash(tsize.NextSize(tid), tid);
        }

        protected void removedIncrement(uint tid)
        {
            int _tsize = tsize[tid];
            --count;
            tcount.Decrement(tid);
            if (++removed > (_tsize * REMOVED_PERCENT_LIMIT))
            {
                if (_tsize < _tsize / 2)
                    Rehash(tsize.PreviousSize(tid), tid);
                else
                    Rehash(_tsize, tid);
            }
        }

        protected void removedDecrement()
        {
            ++count;
            --removed;
        }

        public TetraSeriesBase(int capacity = 16, HashBits bits = HashBits.bit64) : base(bits)
        {
            size = capacity;
            minSize = capacity;
            tsize = new TetraSize(capacity);
            tcount = new TetraCount();
            table = new TetraTable<V>(this, capacity);
            first = EmptyItem();
            last = first;
            ValueEquals = getValueComparer();
        }

        public TetraSeriesBase(
            IList<ISeriesItem<V>> collection,
            int capacity = 16,
            HashBits bits = HashBits.bit64
        ) : this(capacity > collection.Count ? capacity : collection.Count, bits)
        {
            this.Add(collection);
        }

        public TetraSeriesBase(
            IEnumerable<ISeriesItem<V>> collection,
            int capacity = 16,
            HashBits bits = HashBits.bit64
        ) : this(capacity, bits)
        {
            this.Add(collection);
        }

        public virtual ISeriesItem<V> First => first;
        public virtual ISeriesItem<V> Last => last;

        public virtual int Size => size;
        public virtual int Count => count;
        public virtual int MinCount { get; set; }
        public virtual bool IsReadOnly { get; set; }
        public virtual bool IsSynchronized { get; set; }
        public virtual bool IsRepeatable
        {
            get => false;
        }
        public virtual object SyncRoot { get; set; }
        public virtual Func<V, V, bool> ValueEquals { get; }

        ISeriesItem<V> IList<ISeriesItem<V>>.this[int index]
        {
            get => GetItem(index);
            set => GetItem(index).Set(value);
        }
        public virtual V this[int index]
        {
            get => GetItem(index).Value;
            set => GetItem(index).Value = value;
        }
        protected V this[ulong hashkey]
        {
            get { return InnerGet(hashkey); }
            set { InnerPut(hashkey, value); }
        }
        public virtual V this[object key]
        {
            get { return InnerGet(unique.Key(key)); }
            set { InnerPut(unique.Key(key), value); }
        }
        object IFindableSeries.this[object key]
        {
            get => InnerGet(unique.Key(key));
            set => InnerPut(unique.Key(key), (V)value);
        }

        public virtual V InnerGet(ulong key)
        {
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];

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

        public V Get(ulong key)
        {
            return InnerGet(key);
        }

        public virtual V Get(object key)
        {
            return InnerGet(unique.Key(key));
        }

        public virtual V Get(IUnique<V> key)
        {
            return InnerGet(unique.Key(key));
        }

        public virtual V Get(IUnique key)
        {
            return InnerGet(unique.Key(key));
        }

        public virtual bool InnerTryGet(ulong key, out ISeriesItem<V> output)
        {
            output = null;
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];
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

        public virtual bool TryGet(object key, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key), out output);
        }

        public virtual bool TryGet(object key, out V output)
        {
            output = default(V);
            ISeriesItem<V> item = null;
            if (InnerTryGet(unique.Key(key), out item))
            {
                output = item.Value;
                return true;
            }
            return false;
        }

        public bool TryGet(ulong key, out V output)
        {
            output = default(V);
            ISeriesItem<V> item = null;
            if (InnerTryGet(key, out item))
            {
                output = item.Value;
                return true;
            }
            return false;
        }

        public bool TryGet(IUnique key, out ISeriesItem<V> output)
        {
            return catalogImplementation.TryGet(key, out output);
        }

        public bool TryGet(IUnique<V> key, out ISeriesItem<V> output)
        {
            return catalogImplementation.TryGet(key, out output);
        }

        public virtual ISeriesItem<V> InnerGetItem(ulong key)
        {
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];
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

            return null;
        }

        public abstract ISeriesItem<V> GetItem(int index);

        public virtual ISeriesItem<V> GetItem(object key)
        {
            return InnerGetItem(unique.Key(key));
        }

        public ISeriesItem<V> GetItem(ulong key)
        {
            return InnerGetItem(key);
        }

        public ISeriesItem<V> GetItem(IUnique key)
        {
            return catalogImplementation.GetItem(key);
        }

        public ISeriesItem<V> GetItem(IUnique<V> key)
        {
            return catalogImplementation.GetItem(key);
        }

        public ISeriesItem<V> Set(object key, V value)
        {
            return catalogImplementation.Set(key, value);
        }

        public ISeriesItem<V> Set(ulong key, V value)
        {
            return catalogImplementation.Set(key, value);
        }

        public ISeriesItem<V> Set(IUnique key, V value)
        {
            return catalogImplementation.Set(key, value);
        }

        public ISeriesItem<V> Set(IUnique<V> key, V value)
        {
            return catalogImplementation.Set(key, value);
        }

        public ISeriesItem<V> Set(V value)
        {
            return catalogImplementation.Set(value);
        }

        public ISeriesItem<V> Set(IUnique<V> value)
        {
            return catalogImplementation.Set(value);
        }

        public ISeriesItem<V> Set(ISeriesItem<V> value)
        {
            return catalogImplementation.Set(value);
        }

        public int Set(IEnumerable<V> values)
        {
            return catalogImplementation.Set(values);
        }

        public int Set(IList<V> values)
        {
            return catalogImplementation.Set(values);
        }

        public int Set(IEnumerable<ISeriesItem<V>> values)
        {
            return catalogImplementation.Set(values);
        }

        public int Set(IEnumerable<IUnique<V>> values)
        {
            return catalogImplementation.Set(values);
        }

        public ISeriesItem<V> EnsureGet(object key, Func<ulong, V> sureaction)
        {
            return catalogImplementation.EnsureGet(key, sureaction);
        }

        public ISeriesItem<V> EnsureGet(ulong key, Func<ulong, V> sureaction)
        {
            return catalogImplementation.EnsureGet((object)key, sureaction);
        }

        public ISeriesItem<V> EnsureGet(IUnique key, Func<ulong, V> sureaction)
        {
            return catalogImplementation.EnsureGet((object)key, sureaction);
        }

        public ISeriesItem<V> EnsureGet(IUnique<V> key, Func<ulong, V> sureaction)
        {
            return catalogImplementation.EnsureGet((object)key, sureaction);
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

        protected virtual Func<V, V, bool> getValueComparer()
        {
            if (typeof(V).IsValueType)
                return (o1, o2) => o1.Equals(o2);
            return (o1, o2) => ReferenceEquals(o1, o2);
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

        public virtual ISeriesItem<V> Put(ISeriesItem<V> _item)
        {
            return InnerPut(_item);
        }

        public virtual void Put(IList<ISeriesItem<V>> items)
        {
            int c = items.Count;
            for (int i = 0; i < c; i++)
            {
                InnerPut(items[i]);
            }
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
            int c = items.Count;
            for (int i = 0; i < c; i++)
            {
                Put(items[i]);
            }
        }

        public virtual void Put(IEnumerable<V> items)
        {
            foreach (V item in items)
                Put(item);
        }

        public virtual ISeriesItem<V> Put(IUnique<V> value)
        {
            if (value is ISeriesItem<V>)
                return InnerPut((ISeriesItem<V>)value);
            return InnerPut(value.CompactKey(), value.UniqueObject);
        }

        public virtual void Put(IList<IUnique<V>> value)
        {
            foreach (IUnique<V> item in value)
            {
                Put(item);
            }
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
            int c = itemList.Count;
            for (int i = 0; i < c; i++)
            {
                InnerAdd(itemList[i]);
            }
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

        public virtual void Add(IList<V> items)
        {
            int c = items.Count;
            for (int i = 0; i < c; i++)
            {
                Add(items[i]);
            }
        }

        public virtual void Add(IEnumerable<V> items)
        {
            foreach (V item in items)
                Add(item);
        }

        public virtual void Add(IUnique<V> value)
        {
            if (value is ISeriesItem<V>)
                InnerAdd((ISeriesItem<V>)value);
            InnerAdd(unique.Key(value), value.UniqueObject);
        }

        public virtual void Add(IList<IUnique<V>> value)
        {
            foreach (IUnique<V> item in value)
            {
                Add(item);
            }
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

        public virtual void Insert(int index, ISeriesItem<V> item)
        {
            ulong key = item.Key;
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            ulong pos = getPosition(key);
            var _table = table[tid];
            ISeriesItem<V> _item = _table[pos];
            if (_item == null)
            {
                _item = NewItem(item);
                _table[pos] = _item;
                InnerInsert(index, _item);
                countIncrement(tid);
                return;
            }

            for (; ; )
            {
                if (_item.Equals(key))
                {
                    if (_item.Removed)
                    {
                        var newitem = NewItem(item);
                        _item.Extended = newitem;
                        InnerInsert(index, newitem);
                        conflictIncrement(tid);
                        return;
                    }
                    throw new Exception("SeriesItem exist");
                }

                if (_item.Extended == null)
                {
                    var newitem = NewItem(item);
                    _item.Extended = newitem;
                    InnerInsert(index, newitem);
                    conflictIncrement(tid);
                    return;
                }
                item = item.Extended;
            }
        }

        public virtual void Insert(int index, V item)
        {
            Insert(index, NewItem(unique.Key(item), item));
        }

        public virtual bool Enqueue(V value)
        {
            return InnerAdd(unique.Key(value), value);
        }

        public virtual bool Enqueue(object key, V value)
        {
            return InnerAdd(unique.Key(key), value);
        }

        public virtual void Enqueue(ISeriesItem<V> item)
        {
            InnerAdd(item.Key, item.Value);
        }

        public virtual bool TryTake(out V output)
        {
            return TryDequeue(out output);
        }

        public bool TryPick(int skip, out V output)
        {
            return catalogImplementation.TryPick(skip, out output);
        }

        public virtual V Dequeue()
        {
            V item = default(V);
            TryDequeue(out item);
            return item;
        }

        public virtual bool TryDequeue(out V output)
        {
            var _output = Next(first);
            if (_output != null)
            {
                _output.Removed = true;
                removedIncrement(getTetraId(_output.Key));
                output = _output.Value;
                return true;
            }
            output = default(V);
            return false;
        }

        public virtual bool TryDequeue(out ISeriesItem<V> output)
        {
            output = Next(first);
            if (output != null)
            {
                output.Removed = true;
                removedIncrement(getTetraId(output.Key));
                return true;
            }
            return false;
        }

        private void renewClear(int capacity)
        {
            if (capacity != size || count > 0)
            {
                size = capacity;
                conflicts = 0;
                removed = 0;
                count = 0;
                tcount.ResetAll();
                tsize.ResetAll();
                table = new TetraTable<V>(this, size);
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
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];

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

        public virtual V InnerRemove(ulong key)
        {
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (!mem.Removed)
                    {
                        mem.Removed = true;
                        removedIncrement(getTetraId(mem.Key));
                        return mem.Value;
                    }
                    return default(V);
                }

                mem = mem.Extended;
            }
            return default(V);
        }

        protected virtual V InnerRemove(ulong key, V item)
        {
            uint tid = getTetraId(key);
            int _size = tsize[tid];
            uint pos = (uint)getPosition(key);

            ISeriesItem<V> mem = table[tid, pos];

            while (mem != null)
            {
                if (mem.Equals(key))
                {
                    if (mem.Removed)
                        return default(V);

                    if (ValueEquals(mem.Value, item))
                    {
                        mem.Removed = true;
                        removedIncrement(getTetraId(mem.Key));
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
            return TryRemove(unique.Key(item));
        }

        public virtual bool TryRemove(object key)
        {
            return InnerRemove(unique.Key(key)).Equals(default(V)) ? false : true;
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
            conflicts = 0;
            removed = 0;
            count = 0;
            tcount.ResetAll();
            tsize.ResetAll();
            table = new TetraTable<V>(this, size);
            first = EmptyItem();
            last = first;
        }

        public void Resize(int size)
        {
            catalogImplementation.Resize(size);
        }

        public virtual void Flush()
        {
            conflicts = 0;
            removed = 0;
            count = 0;
            tcount.ResetAll();
            tsize.ResetAll();
            table = new TetraTable<V>(this, size);
            first = EmptyItem();
            last = first;
        }

        public virtual void CopyTo(ISeriesItem<V>[] array, int index)
        {
            int c = count,
                i = index,
                l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (ISeriesItem<V> ves in this.AsItems().Take(c))
                    array[i++] = ves;
            }
            else
                foreach (ISeriesItem<V> ves in this)
                    array[i++] = ves;
        }

        public virtual void CopyTo(Array array, int index)
        {
            int c = count,
                i = index,
                l = array.Length;
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
            int c = count,
                i = index,
                l = array.Length;
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

        public virtual void CopyTo(IUnique<V>[] array, int arrayIndex)
        {
            int c = count,
                i = arrayIndex,
                l = array.Length;
            if (l - i < c)
            {
                c = l - i;
                foreach (ISeriesItem<V> ves in this.AsItems().Take(c))
                    array[i++] = ves;
            }
            else
                foreach (ISeriesItem<V> ves in this)
                    array[i++] = ves;
        }

        public virtual V[] ToArray()
        {
            return this.AsValues().ToArray();
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

        public virtual void Resize(int size, uint tid)
        {
            Rehash(size, tid);
        }

        public abstract ISeriesItem<V> EmptyItem();

        public abstract ISeriesItem<V> NewItem(ulong key, V value);
        public abstract ISeriesItem<V> NewItem(object key, V value);
        public abstract ISeriesItem<V> NewItem(ISeriesItem<V> item);
        public abstract ISeriesItem<V> NewItem(V item);

        public abstract ISeriesItem<V>[] EmptyItemTable(int size);

        public virtual int IndexOf(ISeriesItem<V> item)
        {
            return GetItem(item).Index;
        }

        public virtual int IndexOf(V item)
        {
            return GetItem(item).Index;
        }

        public virtual IEnumerable<V> AsValues()
        {
            return (IEnumerable<V>)this;
        }

        public virtual IEnumerable<ISeriesItem<V>> AsItems()
        {
            return (IEnumerable<ISeriesItem<V>>)this;
        }

        public virtual IEnumerable<IUnique<V>> AsIdentifiers()
        {
            return (IEnumerable<IUnique<V>>)this;
        }

        public virtual IEnumerator<ISeriesItem<V>> GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }

        IEnumerator<V> IEnumerable<V>.GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }

        IEnumerator<IUnique<V>> IEnumerable<IUnique<V>>.GetEnumerator()
        {
            return new SeriesItemKeyEnumerator<V>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
        }

        protected static uint getTetraId(ulong key)
        {
            return (uint)(((long)key & 1L) - (((long)key & -1L) * 2));
        }

        protected ulong getPosition(ulong key)
        {
            return (key % (uint)(size - 1));
        }

        protected static ulong getPosition(ulong key, int newsize)
        {
            return (key % (uint)(newsize - 1));
        }

        protected virtual void Rehash(int newsize, uint tid)
        {
            int finish = tcount[tid];
            int _tsize = tsize[tid];
            ISeriesItem<V>[] newItemTable = EmptyItemTable(newsize);
            ISeriesItem<V> item = first;
            item = item.Next;
            if (removed > 0)
            {
                rehashAndReindex(item, newItemTable, newsize, tid);
            }
            else
            {
                rehashOnly(item, newItemTable, newsize, tid);
            }

            table[tid] = newItemTable;
            size = newsize - _tsize;
        }

        private void rehashAndReindex(
            ISeriesItem<V> item,
            ISeriesItem<V>[] newItemTable,
            int newsize,
            uint tid
        )
        {
            int _conflicts = 0;
            int _oldconflicts = 0;
            int _removed = 0;
            ISeriesItem<V>[] _newItemTable = newItemTable;
            ISeriesItem<V> _firstitem = EmptyItem();
            ISeriesItem<V> _lastitem = _firstitem;
            do
            {
                if (!item.Removed)
                {
                    ulong pos = getPosition(item.Key, newsize);

                    ISeriesItem<V> mem = _newItemTable[pos];

                    if (mem == null)
                    {
                        if (item.Extended != null)
                            _oldconflicts++;

                        item.Extended = null;
                        _newItemTable[pos] = _lastitem = _lastitem.Next = item;
                    }
                    else
                    {
                        for (; ; )
                        {
                            if (mem.Extended == null)
                            {
                                item.Extended = null;
                                ;
                                _lastitem = _lastitem.Next = mem.Extended = item;
                                _conflicts++;
                                break;
                            }
                            else
                                mem = mem.Extended;
                        }
                    }
                }
                else
                    _removed++;

                item = item.Next;
            } while (item != null);
            conflicts -= _oldconflicts;
            removed -= _removed;
            first = _firstitem;
            last = _lastitem;
        }

        private void rehashOnly(
            ISeriesItem<V> item,
            ISeriesItem<V>[] newItemTable,
            int newsize,
            uint tid
        )
        {
            int _conflicts = 0;
            int _oldconflicts = 0;
            ISeriesItem<V>[] _newItemTable = newItemTable;
            do
            {
                if (!item.Removed)
                {
                    ulong pos = getPosition(item.Key, newsize);

                    ISeriesItem<V> mem = _newItemTable[pos];

                    if (mem == null)
                    {
                        if (item.Extended != null)
                            _oldconflicts++;

                        item.Extended = null;
                        _newItemTable[pos] = item;
                    }
                    else
                    {
                        for (; ; )
                        {
                            if (mem.Extended == null)
                            {
                                item.Extended = null;
                                mem.Extended = item;
                                _conflicts++;
                                break;
                            }
                            else
                                mem = mem.Extended;
                        }
                    }
                }

                item = item.Next;
            } while (item != null);
            conflicts -= _oldconflicts;
        }

        private bool disposedValue = false;
        private ISeries<V> catalogImplementation;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    first = null;
                    last = null;
                }

                table.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Equals(IUnique? other)
        {
            return code.Equals(other);
        }

        public int CompareTo(IUnique? other)
        {
            return code.CompareTo(other);
        }

        public IUnique Empty => Usid.Empty;

        public new ulong Key
        {
            get => code.Key;
            set => code.Key = value;
        }

        public ulong TypeKey
        {
            get => code.TypeKey;
            set => code.TypeKey = value;
        }

        public byte[] GetBytes()
        {
            return code.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return code.GetKeyBytes();
        }

        public Type ItemType
        {
            get { return typeof(V); }
        }
        public Expression Expression { get; set; }
        public IQueryProvider Provider { get; protected set; }
    }
}
