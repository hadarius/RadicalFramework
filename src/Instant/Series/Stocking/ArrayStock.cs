namespace Radical.Instant.Series.Stocking
{
    using System.Collections;
    using System.Collections.Generic;
    using Extracting;
    using System.Runtime.InteropServices;
    using System;
    using Options;

    public class ArrayStock<T> : ArrayStock, IStock<T> where T : struct
    {
        public ArrayStock() : this(new StockOptions()) { }

        public ArrayStock(StockOptions options)
            : base(
                $"{options.BasePath}/{typeof(T).Name}",
                $"{options.BasePath}/{typeof(T).FullName}",
                options.BlockSize,
                options.SectorSize,
                typeof(T),
                options.IsOwner
            )
        {
            options.Type = typeof(T);
        }
    }

    public unsafe class ArrayStock : Stock, IList<object>, IStock
    {
        public int Length { get; private set; }

        public object this[int index]
        {
            get
            {
                object item = null;
                return Read(item, index, _type);
            }
            set { Write(value, index, _type); }
        }

        public object this[int index, int field, Type t]
        {
            get
            {
                object item = null;
                return Read(item, index, t, 1000);
            }
            set { Write(value, index, t, 1000); }
        }

        public void Rewrite(int index, object structure)
        {
            Read(structure, index);
        }

        private readonly int _itemSize;
        private readonly Type _type;

        public ArrayStock(
            string file,
            string name,
            int itemSize,
            int length,
            Type t,
            bool isOwner
        ) : base(file, name, itemSize, length, isOwner, true)
        {
            _type = t;
            Length = length;
            _itemSize = itemSize;
            Open();
        }

        public ArrayStock(string file, string name, int itemSize, int length, Type t)
            : this(file, name, itemSize, length, t, true) { }

        public ArrayStock(string file, string name, int length, int itemSize)
            : this(file, name, itemSize, length, typeof(byte[]), true) { }

        public ArrayStock(string file, string name, Type t)
            : this(file, name, Marshal.SizeOf(t), 0, t, false) { }

        public ArrayStock(SectorOptions options) : base(options)
        {
            SectorId = options.SectorId;
            ClusterId = options.ClusterId;
            _type = options.ItemType;
            Length = options.SectorSize;
            _itemSize = options.BlockSize;
            Open();
        }

        protected override bool DoOpen()
        {
            if (!IsOwnerOfSharedMemory)
            {
                if (BufferSize % _itemSize != 0)
                    throw new ArgumentOutOfRangeException(
                        "name",
                        "BufferSize is not evenly divisible by the size of " + _type.Name
                    );

                Length = (int)(BufferSize / _itemSize);
            }

            return true;
        }

        public new void Write(object data, long position = 0, Type t = null, int timeout = 1000)
        {
            if (position > Length - 1 || position < 0)
                throw new ArgumentOutOfRangeException("index");
            if (t == null)
                t = _type;
            base.Write(data, position * _itemSize, t, timeout);
        }

        public new void Write(object[] buffer, long position = 0, Type t = null, int timeout = 1000)
        {
            if (t == null)
                t = _type;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length + position > Length || position < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Write(buffer, position * _itemSize, t, timeout);
        }

        public new void Write(
            object[] buffer,
            int index,
            int count,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            if (t == null)
                t = _type;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length - index < count)
                count = buffer.Length - index;
            if (count + position > Length || position < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Write(buffer, index, count, position * _itemSize, t, timeout);
        }

        public new void Write(
            IntPtr ptr,
            long length,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            base.Write(ptr, length, (position * _itemSize), t, timeout);
        }

        public new void Write(
            byte* ptr,
            long length,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            base.Write(ptr, length, (position * _itemSize), t, timeout);
        }

        public new object Read(object data, long position = 0, Type t = null, int timeout = 1000)
        {
            if (t == null)
                t = _type;
            if (position > Length - 1 || position < 0)
                throw new ArgumentOutOfRangeException("index");

            return base.Read(data, (position * _itemSize), t, timeout);
        }

        public new void Read(object[] buffer, long position = 0, Type t = null, int timeout = 1000)
        {
            if (t == null)
                t = _type;
            if (buffer == null)
                throw new ArgumentOutOfRangeException("buffer");
            if (Length - position < 0 || position < 0)
                position = 0;

            if (buffer.Length + position > Length || position < 0)
                throw new ArgumentOutOfRangeException("index");

            base.Read(buffer, position * _itemSize, t, timeout);
        }

        public new void Read(
            object[] buffer,
            int index,
            int count,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            if (t == null)
                t = _type;
            if (buffer == null)
                throw new ArgumentOutOfRangeException("buffer");
            if (Length - position < 0 || position < 0)
                position = 0;

            if (buffer.Length - index < count)
                count = buffer.Length - index;

            if (count + position > Length || position < 0)
                throw new ArgumentOutOfRangeException("index");

            base.Read(buffer, index, count, position * _itemSize, t, timeout);
        }

        public new void Read(
            IntPtr destination,
            long length,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            if (t == null)
                t = _type;
            base.Read(destination, length, (position * _itemSize), t, timeout);
        }

        public new void Read(
            byte* destination,
            long length,
            long position = 0,
            Type t = null,
            int timeout = 1000
        )
        {
            if (t == null)
                t = _type;
            base.Read(destination, length, (position * _itemSize), t, timeout);
        }

        public void CopyTo(object[] buffer, int position = 0)
        {
            if (buffer == null)
            {
                if (Length - position < 0 || position < 0)
                    position = 0;
                buffer = new object[Length - position];
            }

            if (buffer.Length + position > Length || position < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Read(buffer, position * _itemSize);
        }

        public void CopyTo(IStock destination, uint length, int position = 0)
        {
            Extract.CopyBlock(destination.GetStockPtr() + position, this.GetStockPtr(), length);
        }

        public IEnumerator<object> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(object item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
