namespace Radical.Instant.Rubrics
{
    using System.Runtime.InteropServices;
    using Radical.Series;
    using Radical.Uniques;
    using Radical.Series.Base;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class RubricItem : SeriesItemBase<MemberRubric>
    {
        private ulong _key;

        public RubricItem() { }

        public RubricItem(ISeriesItem<MemberRubric> value) : base(value) { }

        public RubricItem(MemberRubric value) : base(value) { }

        public RubricItem(object key, MemberRubric value) : base(key, value) { }

        public RubricItem(ulong key, MemberRubric value) : base(key, value) { }

        public override ulong Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public override int CompareTo(ISeriesItem<MemberRubric> other)
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

        public override void Set(ISeriesItem<MemberRubric> item)
        {
            this.value = item.Value;
            _key = item.Key;
        }

        public override void Set(MemberRubric value)
        {
            this.value = value;
            _key = value.Key;
        }

        public override void Set(object key, MemberRubric value)
        {
            this.value = value;
            _key = key.UniqueKey64();
        }
    }
}
