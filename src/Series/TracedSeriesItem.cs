namespace Radical.Series
{
    using Radical.Instant;
    using System.Runtime.InteropServices;
    using Radical.Uniques;
    using Base;
    using Instant.Updating;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class TracedSeriesItem<V> : SeriesItemBase<V> where V : class, ITracedSeries
    {
        private ulong _key;
        private Updater<V> _proxy;

        public TracedSeriesItem() { }

        public TracedSeriesItem(ISeriesItem<V> value) : base(value) { }

        public TracedSeriesItem(object key, V value) : base(key, value) { }

        public TracedSeriesItem(ulong key, V value) : base(key, value) { }

        public TracedSeriesItem(V value) : base(value) { }

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
            if (this.value == null)
            {
                _proxy = new Updater<V>(item.Value);
                value = _proxy.Preset;
            }
            else
            {
                value.PatchTo(_proxy.EntryProxy);
            }

            _key = item.Key;
        }

        public override void Set(object key, V value)
        {
            if (this.value == null)
            {
                _proxy = new Updater<V>(value);
                this.value = _proxy.Entry;
            }
            else
            {
                value.PatchTo(_proxy.EntryProxy);
            }

            _key = key.UniqueKey64();
        }

        public override void Set(V value)
        {
            Set(value.UniqueKey64(), value);
        }

        public override V UniqueObject
        {
            get => base.UniqueObject;
            set => value.PatchTo(_proxy.EntryProxy);
        }

        public override V Value
        {
            get => base.Value;
            set => value.PatchTo(_proxy.EntryProxy);
        }
    }
}
