namespace Radical.Series.Base
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Radical.Uniques;
    using Enumerators;

    public abstract class TypedSeriesBase<V> : UniqueKey, ITypedSeries<V> where V : IUnique
    {
        internal const float RESIZING_VECTOR = 1.766f;
        internal const float CONFLICTS_PERCENT_LIMIT = 0.22f;
        internal const float REMOVED_PERCENT_LIMIT = 0.15f;

        protected Uscn serialcode;
        protected ISeriesItem<V> first,
            last;
        protected ISeriesItem<V>[] table;
        protected int count,
            conflicts,
            removed,
            minSize,
            size,
            mincount;
        protected uint maxId;

        protected int nextSize()
        {
            return (((int)(size * RESIZING_VECTOR)) ^ 3);
        }

        protected int previousSize()
        {
            return (int)(size * (1 - REMOVED_PERCENT_LIMIT)) ^ 3;
        }

        protected void countIncrement()
        {
            if ((++count + 7) > size)
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

        public TypedSeriesBase(int capacity = 17, HashBits bits = HashBits.bit64) : base(bits)
        {
            size = capacity;
            minSize = capacity;
            maxId = (uint)(size - 1);
            table = EmptyTable(capacity);
            first = EmptyItem();
            last = first;
            ValueEquals = getValueComparer();
            serialcode = new Uscn(typeof(V).UniqueKey64());
        }

        public TypedSeriesBase(
            IList<ISeriesItem<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity > collection.Count ? capacity : collection.Count, bits)
        {
            this.Add(collection);
        }

        public TypedSeriesBase(
            IList<IUnique<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity > collection.Count ? capacity : collection.Count, bits)
        {
            foreach (var c in collection)
                this.Add(c);
        }

        public TypedSeriesBase(
            IEnumerable<ISeriesItem<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity, bits)
        {
            this.Add(collection);
        }

        public TypedSeriesBase(
            IEnumerable<IUnique<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity, bits)
        {
            foreach (var c in collection)
                this.Add(c);
        }

        public virtual ISeriesItem<V> First
        {
            get { return first; }
        }
        public virtual ISeriesItem<V> Last
        {
            get { return last; }
        }

        public virtual int Size
        {
            get => size;
        }
        public virtual int Count
        {
            get => count;
        }
        public virtual int MinCount
        {
            get => mincount;
            set => mincount = value;
        }
        public virtual bool IsReadOnly { get; set; }
        public virtual bool IsSynchronized { get; set; }
        public virtual bool IsRepeatable
        {
            get => false;
        }
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
            get
            {
                if (key is IUnique)
                {
                    IUnique ukey = (IUnique)key;
                    return InnerGet(unique.Key(ukey, ukey.TypeKey));
                }
                else
                    throw new NotSupportedException();
            }
            set
            {
                if (key is IUnique)
                {
                    IUnique ukey = (IUnique)key;
                    InnerPut(unique.Key(ukey, ukey.TypeKey), value);
                }
                else
                    throw new NotSupportedException();
            }
        }
        object IFindableSeries.this[object key]
        {
            get
            {
                if (key is IUnique)
                {
                    IUnique ukey = (IUnique)key;
                    return InnerGet(unique.Key(ukey, ukey.TypeKey));
                }
                else
                    throw new NotSupportedException();
            }
            set
            {
                if (key is IUnique)
                {
                    IUnique ukey = (IUnique)key;
                    InnerPut(unique.Key(ukey, ukey.TypeKey), (V)value);
                }
                else
                    throw new NotSupportedException();
            }
        }
        public virtual V this[IUnique key]
        {
            get => InnerGet(unique.Key(key, key.TypeKey));
            set => InnerPut(unique.Key(key, key.TypeKey), value);
        }
        public virtual V this[IUnique<V> key]
        {
            get => InnerGet(unique.Key(key, key.TypeKey));
            set => InnerPut(unique.Key(key, key.TypeKey), value);
        }
        public virtual V this[object key, ulong seed]
        {
            get => InnerGet(unique.Key(key, seed));
            set => InnerPut(unique.Key(key, seed), value);
        }
        public virtual V this[IUnique key, ulong seed]
        {
            get => InnerGet(unique.Key(key, seed));
            set => InnerPut(unique.Key(key, seed), value);
        }
        public virtual V this[IUnique<V> key, ulong seed]
        {
            get => InnerGet(unique.Key(key, seed));
            set => InnerPut(unique.Key(key, seed), value);
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
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                return InnerGet(unique.Key(ukey, ukey.TypeKey));
            }
            else
                throw new NotSupportedException();
        }

        public virtual V Get(object key, ulong seed)
        {
            return InnerGet(unique.Key(key, seed));
        }

        public virtual V Get(IUnique key)
        {
            return InnerGet(unique.Key(key, key.TypeKey));
        }

        public virtual V Get(IUnique<V> key)
        {
            return InnerGet(unique.Key(key, key.TypeKey));
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
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                return InnerTryGet(unique.Key(ukey, ukey.TypeKey), out output);
            }
            else
                throw new NotSupportedException();
        }

        public virtual bool TryGet(object key, out V output)
        {
            if (key is IUnique)
            {
                output = default(V);
                ISeriesItem<V> item = null;
                IUnique ukey = (IUnique)key;
                if (InnerTryGet(unique.Key(ukey, ukey.TypeKey), out item))
                {
                    output = item.Value;
                    return true;
                }
                return false;
            }
            else
                throw new NotSupportedException();
        }

        public virtual bool TryGet(object key, ulong seed, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key, seed), out output);
        }

        public virtual bool TryGet(object key, ulong seed, out V output)
        {
            output = default(V);
            ISeriesItem<V> item = null;
            if (InnerTryGet(unique.Key(key, seed), out item))
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

        public bool TryGet(IUnique key, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key, key.TypeKey), out output);
        }

        public bool TryGet(IUnique<V> key, out ISeriesItem<V> output)
        {
            return InnerTryGet(unique.Key(key, key.TypeKey), out output);
        }

        protected virtual ISeriesItem<V> InnerGetItem(ulong key)
        {
            if (key == 0)
                return null;

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

            return null;
        }

        public virtual ISeriesItem<V> GetItem(ulong key)
        {
            return InnerGetItem(key);
        }

        public ISeriesItem<V> GetItem(IUnique key)
        {
            return InnerGetItem(unique.Key(key, key.TypeKey));
        }

        public ISeriesItem<V> GetItem(IUnique<V> key)
        {
            return InnerGetItem(unique.Key(key, key.TypeKey));
        }

        protected virtual ISeriesItem<V> InnerSet(ulong key, V value)
        {
            var item = InnerGetItem(key);
            if (item != null)
                item.Value = value;
            return item;
        }

        protected virtual ISeriesItem<V> InnerSet(ISeriesItem<V> value)
        {
            var item = GetItem(value);
            if (item != null)
                item.Value = value.Value;
            return item;
        }

        public ISeriesItem<V> Set(object key, V value)
        {
            return InnerSet(unique.Key(key, value.TypeKey), value);
        }

        public ISeriesItem<V> Set(ulong key, V value)
        {
            return massDeckImplementation.Set(key, value);
        }

        public ISeriesItem<V> Set(IUnique key, V value)
        {
            return InnerSet(unique.Key(key, key.TypeKey), value);
        }

        public ISeriesItem<V> Set(IUnique<V> key, V value)
        {
            return InnerSet(unique.Key(key, key.TypeKey), value);
        }

        public ISeriesItem<V> Set(V value)
        {
            return InnerSet(unique.Key(value, value.TypeKey), value);
        }

        public ISeriesItem<V> Set(IUnique<V> value)
        {
            return InnerSet(unique.Key(value, value.TypeKey), value.UniqueObject);
        }

        public ISeriesItem<V> Set(ISeriesItem<V> value)
        {
            return InnerSet(value);
        }

        public int Set(IEnumerable<V> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (Set(value) != null)
                    count++;
            }

            return count;
        }

        public int Set(IList<V> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (Set(value) != null)
                    count++;
            }

            return count;
        }

        public int Set(IEnumerable<ISeriesItem<V>> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (InnerSet(value) != null)
                    count++;
            }

            return count;
        }

        public int Set(IEnumerable<IUnique<V>> values)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (Set(value) != null)
                    count++;
            }

            return count;
        }

        public ISeriesItem<V> EnsureGet(object key, Func<ulong, V> sureaction)
        {
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                ulong _key = unique.Key(ukey, ukey.TypeKey);
                return (!TryGet(_key, out ISeriesItem<V> item))
                    ? Put(key, sureaction.Invoke(_key))
                    : item;
            }
            else
                throw new NotSupportedException();
        }

        public ISeriesItem<V> EnsureGet(ulong key, Func<ulong, V> sureaction)
        {
            return (!TryGet(key, out ISeriesItem<V> item)) ? Put(key, sureaction.Invoke(key)) : item;
        }

        public ISeriesItem<V> EnsureGet(IUnique key, Func<ulong, V> sureaction)
        {
            ulong _key = unique.Key(key, key.TypeKey);
            return (!TryGet(_key, out ISeriesItem<V> item)) ? Put(key, sureaction.Invoke(_key)) : item;
        }

        public ISeriesItem<V> EnsureGet(IUnique<V> key, Func<ulong, V> sureaction)
        {
            ulong _key = unique.Key(key, key.TypeKey);
            return (!TryGet(_key, out ISeriesItem<V> item)) ? Put(key, sureaction.Invoke(_key)) : item;
        }

        public virtual ISeriesItem<V> GetItem(object key)
        {
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                return InnerGetItem(unique.Key(ukey, ukey.TypeKey));
            }
            else
                throw new NotSupportedException();
        }

        public virtual ISeriesItem<V> GetItem(object key, ulong seed)
        {
            return InnerGetItem(unique.Key(key, seed));
        }

        public abstract ISeriesItem<V> GetItem(int index);

        protected virtual ISeriesItem<V> InnerPut(ulong key, ulong seed, V value)
        {
            value.TypeKey = seed;
            value.Key = key;
            return InnerPut(value);
        }

        protected abstract ISeriesItem<V> InnerPut(ulong key, V value);

        protected virtual ISeriesItem<V> InnerPut(V value, ulong seed)
        {
            value.TypeKey = seed;
            return InnerPut(value);
        }

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
            return InnerPut(unique.Key(key, value.TypeKey), value);
        }

        public virtual ISeriesItem<V> Put(object key, ulong seed, V value)
        {
            return InnerPut(unique.Key(key, seed), value);
        }

        public virtual ISeriesItem<V> Put(object key, ulong seed, object value)
        {
            if (value is V)
            {
                V o = (V)value;
                return InnerPut(unique.Key(key, seed), (V)value);
            }
            return null;
        }

        public virtual ISeriesItem<V> Put(ISeriesItem<V> item)
        {
            return InnerPut(item);
        }

        public virtual void Put(IList<ISeriesItem<V>> items)
        {
            int i = 0,
                c = items.Count;
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

        public virtual void Put(IList<V> items)
        {
            int i = 0,
                c = items.Count;
            while (i < c)
                InnerPut(items[i++]);
        }

        public virtual void Put(IEnumerable<V> items)
        {
            foreach (V item in items)
                Put(item);
        }

        public virtual ISeriesItem<V> Put(V value, ulong seed)
        {
            return InnerPut(value, seed);
        }

        public virtual void Put(object value, ulong seed)
        {
            if (value is IUnique)
            {
                IUnique v = (IUnique)value;
                Put(v, seed);
            }
            else if (value is V)
                Put((V)value, seed);
        }

        public virtual void Put(IList<V> items, ulong seed)
        {
            int c = items.Count;
            for (int i = 0; i < c; i++)
            {
                InnerPut(items[i], seed);
            }
        }

        public virtual void Put(IEnumerable<V> items, ulong seed)
        {
            foreach (V item in items)
                InnerPut(item, seed);
        }

        public virtual ISeriesItem<V> Put(IUnique<V> value)
        {
            return InnerPut(unique.Key(value, value.TypeKey), value.UniqueObject);
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

        protected virtual bool InnerAdd(ulong key, ulong seed, V value)
        {
            value.TypeKey = seed;
            value.Key = key;
            return InnerAdd(value);
        }

        protected abstract bool InnerAdd(ulong key, V value);

        protected virtual bool InnerAdd(V value, ulong seed)
        {
            value.TypeKey = seed;
            return InnerAdd(value);
        }

        protected abstract bool InnerAdd(V value);
        protected abstract bool InnerAdd(ISeriesItem<V> value);

        public virtual bool Add(ulong key, object value)
        {
            V o = (V)value;
            return InnerAdd(key, o.TypeKey, o);
        }

        public virtual bool Add(ulong key, V value)
        {
            return InnerAdd(key, value);
        }

        public virtual bool Add(object key, V value)
        {
            return InnerAdd(unique.Key(key, value.TypeKey), value);
        }

        public virtual bool Add(object key, ulong seed, V value)
        {
            value.TypeKey = seed;
            return InnerAdd(unique.Key(key, seed), value);
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

        public virtual bool Add(V value, ulong seed)
        {
            return InnerAdd(value, seed);
        }

        public virtual void Add(IList<V> items, ulong seed)
        {
            int c = items.Count;
            for (int i = 0; i < c; i++)
            {
                Add(items[i], seed);
            }
        }

        public virtual void Add(IEnumerable<V> items, ulong seed)
        {
            foreach (V item in items)
                Add(item, seed);
        }

        public virtual void Add(IUnique<V> value)
        {
            InnerAdd(unique.Key(value, value.TypeKey), value.UniqueObject);
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

        public virtual bool TryAdd(V value, ulong seed)
        {
            return InnerAdd(value, seed);
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
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                ulong _key = unique.Key(ukey, ukey.TypeKey);
                ISeriesItem<V> newItem = NewItem(_key, default(V));
                if (InnerAdd(newItem))
                    return newItem;
                return null;
            }
            else
                throw new NotSupportedException();
        }

        public virtual ISeriesItem<V> New(object key, ulong seed)
        {
            ISeriesItem<V> newItem = NewItem(unique.Key(key, seed), default(V));
            if (InnerAdd(newItem))
                return newItem;
            return null;
        }

        protected abstract void InnerInsert(int index, ISeriesItem<V> item);

        public virtual void Insert(int index, ISeriesItem<V> item)
        {
            ulong key = item.Key;
            ulong pos = getPosition(key);

            ISeriesItem<V> _item = table[pos];

            if (_item == null)
            {
                _item = NewItem(item);
                table[pos] = _item;
                InnerInsert(index, _item);
                countIncrement();
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
                        conflictIncrement();
                        return;
                    }
                    throw new Exception("SeriesItem exist");
                }

                if (_item.Extended == null)
                {
                    var newitem = NewItem(item);
                    _item.Extended = newitem;
                    InnerInsert(index, newitem);
                    conflictIncrement();
                    return;
                }
                item = item.Extended;
            }
        }

        public virtual void Insert(int index, V item)
        {
            Insert(index, NewItem(item));
        }

        public virtual bool Enqueue(V value)
        {
            return InnerAdd(value);
        }

        public virtual bool Enqueue(object key, V value)
        {
            return Add(key, value);
        }

        public virtual bool Enqueue(V value, ulong seed)
        {
            return InnerAdd(value, seed);
        }

        public virtual bool Enqueue(object key, ulong seed, V value)
        {
            return Add(key, seed, value);
        }

        public virtual void Enqueue(ISeriesItem<V> item)
        {
            InnerAdd(item);
        }

        public virtual bool TryPick(int skip, out V output)
        {
            output = default(V);
            bool check = false;
            if (check = TryPick(skip, out ISeriesItem<V> _output))
                output = _output.Value;
            return check;
        }

        public virtual bool TryPick(int skip, out ISeriesItem<V> output)
        {
            output = this.AsItems().Skip(skip).FirstOrDefault();
            if (output != null)
            {
                return true;
            }
            return false;
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
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                ulong _key = unique.Key(ukey, ukey.TypeKey);
                return InnerContainsKey(_key);
            }
            else
                throw new NotSupportedException();
        }

        public virtual bool ContainsKey(object key, ulong seed)
        {
            return InnerContainsKey(unique.Key(key, seed));
        }

        public virtual bool ContainsKey(ulong key)
        {
            return InnerContainsKey(key);
        }

        public virtual bool ContainsKey(IUnique key)
        {
            return InnerContainsKey(unique.Key(key, key.TypeKey));
        }

        public virtual bool Contains(ISeriesItem<V> item)
        {
            return InnerContainsKey(item.Key);
        }

        public virtual bool Contains(IUnique<V> item)
        {
            return InnerContainsKey(unique.Key(item, item.TypeKey));
        }

        public virtual bool Contains(V item)
        {
            return InnerContainsKey(unique.Key(item, item.TypeKey));
        }

        public virtual bool Contains(V item, ulong seed)
        {
            return InnerContainsKey(unique.Key(item, seed));
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
            return InnerRemove(unique.Key(item, item.TypeKey)).Equals(default(V)) ? false : true;
        }

        public virtual V Remove(object key)
        {
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                ulong _key = unique.Key(ukey, ukey.TypeKey);
                return InnerRemove(_key);
            }
            else
                throw new NotSupportedException();
        }

        public virtual V Remove(object key, ulong seed)
        {
            return InnerRemove(unique.Key(key, seed));
        }

        public virtual bool Remove(ISeriesItem<V> item)
        {
            return InnerRemove(item.Key).Equals(default(V)) ? false : true;
        }

        public virtual bool Remove(IUnique<V> item)
        {
            return TryRemove(unique.Key(item, item.TypeKey));
        }

        public virtual bool TryRemove(object key)
        {
            if (key is IUnique)
            {
                IUnique ukey = (IUnique)key;
                ulong _key = unique.Key(ukey, ukey.TypeKey);
                V result = InnerRemove(unique.Key(key));
                if (result != null && !result.Equals(default(V)))
                    return true;
                return false;
            }
            else
                throw new NotSupportedException();
        }

        public virtual bool TryRemove(object key, ulong seed)
        {
            return InnerRemove(unique.Key(key, seed)).Equals(default(V)) ? false : true;
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
            maxId = (uint)(size - 1);
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

        public virtual V[] ToArray()
        {
            return this.AsValues().ToArray();
        }

        public virtual object[] ToObjectArray()
        {
            return this.AsValues().Select((x) => (object)x).ToArray();
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

        public virtual ISeriesItem<V> NewItem(ulong key, ulong seed, V value)
        {
            value.TypeKey = seed;
            value.Key = key;
            return NewItem(value);
        }

        public abstract ISeriesItem<V> NewItem(ulong key, V value);
        public abstract ISeriesItem<V> NewItem(object key, V value);

        public virtual ISeriesItem<V> NewItem(object key, ulong seed, V value)
        {
            value.TypeKey = seed;
            return NewItem(unique.Key(key, seed), value);
        }

        public abstract ISeriesItem<V> NewItem(ISeriesItem<V> item);

        public virtual ISeriesItem<V> NewItem(V item, ulong seed)
        {
            item.TypeKey = seed;
            return NewItem(item);
        }

        public abstract ISeriesItem<V> NewItem(V item);

        public abstract ISeriesItem<V>[] EmptyTable(int size);

        public virtual int IndexOf(ISeriesItem<V> item)
        {
            return GetItem(item).Index;
        }

        public virtual int IndexOf(V item)
        {
            return GetItem(item).Index;
        }

        protected virtual int IndexOf(ulong key, V value)
        {
            var item = GetItem(key);
            if (ValueEquals(item.Value, value))
                return item.Index;
            return -1;
        }

        public virtual IEnumerable<V> AsValues()
        {
            return (IEnumerable<V>)this;
        }

        public virtual IEnumerable<ISeriesItem<V>> AsItems()
        {
            foreach (ISeriesItem<V> item in this)
            {
                yield return item;
            }
        }

        public virtual IEnumerator<IUnique<V>> GetUniqueEnumerator()
        {
            return new  SeriesItemKeyEnumerator<V>(this);
        }

        public virtual IEnumerator<ISeriesItem<V>> GetEnumerator()
        {
            return new SeriesItemEnumerator<V>(this);
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
            ISeriesItem<V>[] newItemTable = EmptyTable(newsize);
            ISeriesItem<V> item = first;
            item = item.Next;
            if (removed > 0)
            {
                rehashAndReindex(item, newItemTable, newMaxId);
            }
            else
            {
                rehash(item, newItemTable, newMaxId);
            }

            table = newItemTable;
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

                    ISeriesItem<V> mem = _newItemTable[pos];

                    if (mem == null)
                    {
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

                    ISeriesItem<V> mem = _newItemTable[pos];

                    if (mem == null)
                    {
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
            conflicts = _conflicts;
        }

        protected bool disposedValue = false;

        private ITypedSeries<V> massDeckImplementation;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    first = null;
                    last = null;
                }
                table = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Equals(IUnique? other)
        {
            return serialcode.Equals(other);
        }

        public int CompareTo(IUnique? other)
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

        public byte[] GetBytes()
        {
            return serialcode.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return serialcode.GetKeyBytes();
        }
    }
}
