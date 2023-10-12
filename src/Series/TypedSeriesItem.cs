namespace Radical.Series
{
    using Radical.Extracting;
    using System.Runtime.InteropServices;
    using Radical.Uniques;
    using Base;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class TypedSeriesItem<V> : SeriesItemBase<V>
    {
        private ulong _key;

        public TypedSeriesItem() : base() { }

        public TypedSeriesItem(ISeriesItem<V> value) : base(value) { }

        public TypedSeriesItem(object key, V value) : base(key, value) { }

        public TypedSeriesItem(ulong key, V value) : base(key, value) { }

        public TypedSeriesItem(V value) : base(value) { }

        public override ulong Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public override int CompareTo(ISeriesItem<V> other)
        {
            return (int)(Key - other.Key);
        }

        public override int CompareTo(object other)
        {
            return (int)(Key - other.UniqueKey64(TypeKey));
        }

        public override int CompareTo(ulong key)
        {
            return (int)(Key - key);
        }

        public override bool Equals(object y)
        {
            return Key.Equals(y.UniqueKey64(TypeKey));
        }

        public override bool Equals(ulong key)
        {
            return Key == key;
        }

        public override byte[] GetBytes()
        {
            return this.value.GetBytes();
        }

        public override int GetHashCode()
        {
            return (int)Key.UniqueKey32();
        }

        public unsafe override byte[] GetKeyBytes()
        {
            byte[] b = new byte[8];
            fixed (byte* s = b)
                *(ulong*)s = _key;
            return b;
        }

        public override void Set(ISeriesItem<V> item)
        {
            value = item.Value;
            _key = item.Key;
        }

        public override void Set(object key, V value)
        {
            this.value = value;
            _key = key.UniqueKey64(TypeKey);
        }

        public override void Set(V value)
        {
            this.value = value;
            if (this.value is IUnique<V>)
                _key = ((IUnique<V>)value).CompactKey();
        }
    }
}
