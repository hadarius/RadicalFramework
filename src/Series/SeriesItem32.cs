namespace Radical.Series
{
    using System.Runtime.InteropServices;
    using Radical.Uniques;
    using Base;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class SeriesItem32<V> : SeriesItemBase<V>
    {
        private uint _key;

        public SeriesItem32() { }

        public SeriesItem32(ISeriesItem<V> value) : base(value) { }

        public SeriesItem32(object key, V value) : base(key, value) { }

        public SeriesItem32(ulong key, V value) : base(key, value) { }

        public SeriesItem32(V value) : base(value) { }

        public override ulong Key
        {
            get { return _key; }
            set { _key = (uint)value; }
        }

        public override int CompareTo(ISeriesItem<V> other)
        {
            return (int)(Key - other.Key);
        }

        public override int CompareTo(object other)
        {
            return (int)(_key - other.UniqueKey32());
        }

        public override int CompareTo(ulong key)
        {
            return (int)(Key - key);
        }

        public override bool Equals(object y)
        {
            return _key.Equals(y.UniqueKey32());
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
            return (int)_key;
        }

        public unsafe override byte[] GetKeyBytes()
        {
            byte[] b = new byte[4];
            fixed (byte* s = b)
                *(uint*)s = _key;
            return b;
        }

        public override void Set(ISeriesItem<V> item)
        {
            this.value = item.Value;
            _key = (uint)item.Key;
        }

        public override void Set(object key, V value)
        {
            this.value = value;
            _key = key.UniqueKey32();
        }

        public override void Set(V value)
        {
            this.value = value;
            _key = value.UniqueKey32();
        }
    }
}
