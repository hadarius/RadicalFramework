namespace Radical.Series
{
    using System.Runtime.InteropServices;
    using Radical.Uniques;
    using Base;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class SeriesItem<V> : SeriesItemBase<V>
    {
        private ulong _key;

        public SeriesItem() { }

        public SeriesItem(ISeriesItem<V> value) : base(value) { }

        public SeriesItem(object key, V value) : base(key, value) { }

        public SeriesItem(ulong key, V value) : base(key, value) { }

        public SeriesItem(V value) : base(value) { }

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
            return (int)(Key - other.UniqueKey64());
        }

        public override int CompareTo(ulong key)
        {
            return (int)(Key - key);
        }

        public override bool Equals(object y)
        {
            return Key.Equals(y.UniqueKey64());
        }

        public override bool Equals(ulong key)
        {
            return Key == key;
        }

        public override byte[] GetBytes()
        {
            return GetKeyBytes();
        }

        public override int GetHashCode()
        {
            return (int)Key;
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
            this.value = item.Value;
            _key = item.Key;
        }

        public override void Set(object key, V value)
        {
            this.value = value;
            _key = key.UniqueKey64();
        }

        public override void Set(V value)
        {
            this.value = value;
            _key = value.UniqueKey64();
        }
    }
}
