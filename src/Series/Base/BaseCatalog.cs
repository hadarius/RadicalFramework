﻿namespace Radical.Series.Base
{
    using System.Collections.Generic;
    using System.Threading;
    using Radical.Uniques;

    public abstract class BaseCatalog<V> : CatalogSeries<V>
    {
        internal const int WAIT_READ_TIMEOUT = 5000;

        internal const int WAIT_REHASH_TIMEOUT = 5000;

        internal const int WAIT_WRITE_TIMEOUT = 5000;

        int readers;

        internal readonly ManualResetEventSlim readAccess = new ManualResetEventSlim(true, 128);
        internal readonly ManualResetEventSlim rehashAccess = new ManualResetEventSlim(true, 128);
        internal readonly ManualResetEventSlim writeAccess = new ManualResetEventSlim(true, 128);
        internal readonly SemaphoreSlim writePass = new SemaphoreSlim(1);

        protected BaseCatalog() : base() { }

        protected BaseCatalog(int capacity = 17, HashBits bits = HashBits.bit64)
            : base(capacity, bits) { }

        protected BaseCatalog(
            IEnumerable<IUnique<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity, bits)
        {
            if (collection != null)
                foreach (IUnique<V> c in collection)
                    Add(c);
        }

        protected BaseCatalog(
            IEnumerable<V> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this(capacity, bits)
        {
            if (collection != null)
                foreach (V c in collection)
                    Add(c);
        }

        protected BaseCatalog(
            IList<IUnique<V>> collection,
            int capacity = 17,
            HashBits bits = HashBits.bit64
        ) : this((capacity > collection.Count) ? capacity : collection.Count, bits)
        {
            foreach (IUnique<V> c in collection)
                Add(c);
        }

        protected BaseCatalog(IList<V> collection, int capacity = 17, HashBits bits = HashBits.bit64)
            : this((capacity > collection.Count) ? capacity : collection.Count, bits)
        {
            foreach (V c in collection)
                Add(c);
        }

        protected void acquireReader()
        {
            Interlocked.Increment(ref readers);
            rehashAccess.Reset();
            if (!readAccess.Wait(WAIT_READ_TIMEOUT))
                throw new TimeoutException("Wait write Timeout");
        }

        protected void acquireRehash()
        {
            if (!rehashAccess.Wait(WAIT_REHASH_TIMEOUT))
                throw new TimeoutException("Wait write Timeout");
            readAccess.Reset();
        }

        protected void acquireWriter()
        {
            do
            {
                if (!writeAccess.Wait(WAIT_WRITE_TIMEOUT))
                    throw new TimeoutException("Wait write Timeout");
                writeAccess.Reset();
            } while (!writePass.Wait(0));
        }

        protected override V InnerGet(ulong key)
        {
            acquireReader();
            V v = base.InnerGet(key);
            releaseReader();
            return v;
        }

        protected override ISeriesItem<V> InnerGetItem(ulong key)
        {
            acquireReader();
            ISeriesItem<V> item = base.InnerGetItem(key);
            releaseReader();
            return item;
        }

        protected override ISeriesItem<V> InnerPut(ISeriesItem<V> value)
        {
            acquireWriter();
            ISeriesItem<V> temp = base.InnerPut(value);
            releaseWriter();
            return temp;
        }

        protected override ISeriesItem<V> InnerPut(V value)
        {
            acquireWriter();
            ISeriesItem<V> temp = base.InnerPut(value);
            releaseWriter();
            return temp;
        }

        protected override ISeriesItem<V> InnerPut(ulong key, V value)
        {
            acquireWriter();
            ISeriesItem<V> temp = base.InnerPut(key, value);
            releaseWriter();
            return temp;
        }

        protected override V InnerRemove(ulong key)
        {
            acquireWriter();
            V temp = base.InnerRemove(key);
            releaseWriter();
            return temp;
        }

        protected override bool InnerTryGet(ulong key, out ISeriesItem<V> output)
        {
            acquireReader();
            bool test = base.InnerTryGet(key, out output);
            releaseReader();
            return test;
        }

        protected override void Rehash(int newsize)
        {
            acquireRehash();
            base.Rehash(newsize);
            releaseRehash();
        }

        protected override void Reindex()
        {
            acquireRehash();
            base.Reindex();
            releaseRehash();
        }

        protected void releaseReader()
        {
            if (0 == Interlocked.Decrement(ref readers))
                rehashAccess.Set();
        }

        protected void releaseRehash()
        {
            readAccess.Set();
        }

        protected void releaseWriter()
        {
            writePass.Release();
            writeAccess.Set();
        }

        protected override bool InnerAdd(ISeriesItem<V> value)
        {
            acquireWriter();
            bool temp = base.InnerAdd(value);
            releaseWriter();
            return temp;
        }

        protected override bool InnerAdd(V value)
        {
            acquireWriter();
            bool temp = base.InnerAdd(value);
            releaseWriter();
            return temp;
        }

        protected override bool InnerAdd(ulong key, V value)
        {
            acquireWriter();
            bool temp = base.InnerAdd(key, value);
            releaseWriter();
            return temp;
        }

        public override void Clear()
        {
            acquireWriter();
            acquireRehash();

            base.Clear();

            releaseRehash();
            releaseWriter();
        }

        public override void CopyTo(Array array, int index)
        {
            acquireReader();
            base.CopyTo(array, index);
            releaseReader();
        }

        public override void CopyTo(ISeriesItem<V>[] array, int index)
        {
            acquireReader();
            base.CopyTo(array, index);
            releaseReader();
        }

        public override void CopyTo(V[] array, int index)
        {
            acquireReader();
            base.CopyTo(array, index);
            releaseReader();
        }

        public override ISeriesItem<V> GetItem(int index)
        {
            if (index < count)
            {
                acquireReader();
                if (removed > 0)
                {
                    releaseReader();
                    acquireWriter();
                    Reindex();
                    releaseWriter();
                    acquireReader();
                }

                int i = -1;
                int id = index;
                ISeriesItem<V> item = first.Next;
                for (; ; )
                {
                    if (++i == id)
                    {
                        releaseReader();
                        return item;
                    }
                    item = item.Next;
                }
            }
            return null;
        }

        public override int IndexOf(ISeriesItem<V> item)
        {
            int id = 0;
            acquireReader();
            id = base.IndexOf(item);
            releaseReader();
            return id;
        }

        public override int IndexOf(V item)
        {
            int id = 0;
            acquireReader();
            id = base.IndexOf(item);
            releaseReader();
            return id;
        }

        public override void Insert(int index, ISeriesItem<V> item)
        {
            acquireWriter();
            base.Insert(index, item);
            releaseWriter();
        }

        public override V[] ToArray()
        {
            acquireReader();
            V[] array = base.ToArray();
            releaseReader();
            return array;
        }

        public override bool TryDequeue(out ISeriesItem<V> output)
        {
            acquireWriter();
            bool temp = base.TryDequeue(out output);
            releaseWriter();
            return temp;
        }

        public override bool TryDequeue(out V output)
        {
            acquireWriter();
            bool temp = base.TryDequeue(out output);
            releaseWriter();
            return temp;
        }

        public override bool TryPick(int skip, out V output)
        {
            acquireWriter();
            bool temp = base.TryPick(skip, out output);
            releaseWriter();
            return temp;
        }
    }
}
