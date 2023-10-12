namespace Radical.Series.Base
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Uniques;
    using Enumerators;

    [StructLayout(LayoutKind.Sequential)]
    public abstract class SeriesItemBase<V> : ISeriesItem<V>
    {
        protected V value;
        protected bool? isUnique;
        private bool disposedValue = false;
        private ISeriesItem<V> extended;
        private ISeriesItem<V> next;
        private ISeries<V> catalog;

        public SeriesItemBase() { }

        public SeriesItemBase(ISeriesItem<V> value) : base()
        {
            Set(value);
        }

        public SeriesItemBase(object key, V value) : base()
        {
            Set(key, value);
        }

        public SeriesItemBase(ulong key, V value) : base()
        {
            Set(key, value);
        }

        public SeriesItemBase(V value) : base()
        {
            Set(value);
        }

        public virtual IUnique Empty => throw new NotImplementedException();

        public virtual ISeriesItem<V> Extended
        {
            get => extended;
            set => extended = value;
        }

        public virtual int Index { get; set; } = -1;

        public abstract ulong Key { get; set; }

        public virtual ISeriesItem<V> Next
        {
            get => next;
            set => next = value;
        }

        public virtual bool Removed { get; set; }

        public virtual bool Repeated { get; set; }

        public virtual bool IsUnique
        {
            get => isUnique ??= typeof(V).IsAssignableTo(typeof(IUnique));
            set => isUnique = value;
        }

        public virtual V UniqueObject
        {
            get => value;
            set => this.value = value;
        }

        public virtual ulong TypeKey
        {
            get
            {
                if (IsUnique)
                {
                    var uniqueValue = (IUnique)UniqueObject;
                    if (uniqueValue.TypeKey == 0)
                        uniqueValue.TypeKey = typeof(V).UniqueKey32();
                    return uniqueValue.TypeKey;
                }
                return typeof(V).UniqueKey32();
            }
            set
            {
                if (IsUnique)
                {
                    var uniqueValue = (IUnique)UniqueObject;
                    uniqueValue.TypeKey = value;
                }
            }
        }

        public virtual V Value
        {
            get => value;
            set => this.value = value;
        }

        public virtual ulong CompactKey()
        {
            return (IsUnique) ? ((IUnique)UniqueObject).Key : Key;
        }

        public virtual int CompareTo(ISeriesItem<V> other)
        {
            return (int)(Key - other.Key);
        }

        public virtual int CompareTo(IUnique other)
        {
            return (int)(Key - other.Key);
        }

        public abstract int CompareTo(object other);

        public virtual int CompareTo(ulong key)
        {
            return (int)(Key - key);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual bool Equals(ISeriesItem<V> y)
        {
            return this.Equals(y.Key);
        }

        public virtual bool Equals(IUnique other)
        {
            return Key == other.Key;
        }

        public override abstract bool Equals(object y);

        public virtual bool Equals(ulong key)
        {
            return Key == key;
        }

        public abstract byte[] GetBytes();

        public override abstract int GetHashCode();

        public abstract byte[] GetKeyBytes();

        public virtual Type GetUniqueType()
        {
            return typeof(V);
        }

        public abstract void Set(ISeriesItem<V> item);

        public abstract void Set(object key, V value);

        public virtual void Set(ulong key, V value)
        {
            this.value = value;
            Key = key;
        }

        public abstract void Set(V value);

        public virtual int[] UniqueOrdinals()
        {
            return null;
        }

        public virtual object[] UniqueValues()
        {
            return new object[] { Key };
        }

        public virtual ISeriesItem<V> MoveNext(ISeriesItem<V> item)
        {
            ISeriesItem<V> _item = item.Next;
            if (_item != null)
            {
                if (!_item.Removed)
                    return _item;
                return MoveNext(_item);
            }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Value = default(V);
                }

                disposedValue = true;
            }
        }

        IEnumerator<V> IEnumerable<V>.GetEnumerator()
        {
            return new SeriesItemSubEnumerator<V>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SeriesItemSubEnumerator<V>(this);
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
            return new SeriesItemSubEnumerator<V>(this);
        }
    }
}
