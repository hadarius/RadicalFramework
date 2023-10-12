namespace Radical.Instant.Series.Math.Rubrics
{
    using System.Runtime.InteropServices;
    using Radical.Series;
    using Radical.Uniques;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class MathRubricItem : SeriesItem<MathRubric>
    {
        private ulong _key;

        public MathRubricItem() { }

        public MathRubricItem(ISeriesItem<MathRubric> value) : base(value) { }

        public MathRubricItem(long key, MathRubric value) : base(key, value) { }

        public MathRubricItem(MathRubric value) : base(value) { }

        public MathRubricItem(object key, MathRubric value) : base(key.UniqueKey64(), value) { }

        public override ulong Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public override int CompareTo(ISeriesItem<MathRubric> other)
        {
            return (int)(_key - other.Key);
        }

        public override int CompareTo(object other)
        {
            return (int)(_key - other.UniqueKey64());
        }

        public override int CompareTo(ulong key)
        {
            return (int)(_key - key);
        }

        public override bool Equals(object y)
        {
            return _key.Equals(y.UniqueKey64());
        }

        public override bool Equals(ulong key)
        {
            return _key == key;
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
            byte[] b = new byte[8];
            fixed (byte* s = b)
                *(ulong*)s = _key;
            return b;
        }

        public override void Set(ISeriesItem<MathRubric> item)
        {
            this.value = item.Value;
            _key = item.Key;
        }

        public override void Set(MathRubric value)
        {
            this.value = value;
            _key = value.Key;
        }

        public override void Set(object key, MathRubric value)
        {
            this.value = value;
            _key = key.UniqueKey64();
        }
    }
}
