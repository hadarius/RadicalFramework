namespace Radical.Invoking.Work
{
    using Extracting;
    using Series;
    using Uniques;

    public class WorkMethod : SeriesItem<IInvoker>
    {
        private ulong key;

        public WorkMethod() { }

        public WorkMethod(IInvoker value)
        {
            Value = value;
        }

        public WorkMethod(long key, IInvoker value) : base(key, value) { }

        public WorkMethod(object key, IInvoker value) : base(key.UniqueKey64(), value) { }

        public override ulong Key
        {
            get => key;
            set => key = value;
        }

        public override int CompareTo(object other)
        {
            return (int)(Key - other.UniqueKey());
        }

        public override bool Equals(object y)
        {
            return Key == y.UniqueKey64();
        }

        public override byte[] GetBytes()
        {
            return Key.GetBytes();
        }

        public override int GetHashCode()
        {
            return Key.GetBytes().BitAggregate64to32().ToInt32();
        }

        public override byte[] GetKeyBytes()
        {
            return Key.GetBytes();
        }

        public override void Set(ISeriesItem<IInvoker> item)
        {
            Key = item.Key;
            Value = item.Value;
            Removed = false;
        }

        public override void Set(IInvoker value)
        {
            Value = value;
            Removed = false;
        }

        public override void Set(object key, IInvoker value)
        {
            Key = key.UniqueKey64();
            Value = value;
            Removed = false;
        }
    }
}
