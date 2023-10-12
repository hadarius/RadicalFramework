namespace Radical.Instant.Updating
{
    using Invoking;
    using Proxies;

    public class Updater<T> : Updater, IUpdater<T> where T : class
    {
        public Updater() { }

        public Updater(T item) : base(item) { }

        public Updater(T item, IInvoker traceChanges) : base(item, traceChanges) { }

        public T Patch(T item)
        {
            base.Patch<T>(item);
            return item;
        }

        public T PatchFrom(T source)
        {
            ValueArray = creator.Create(source).ValueArray;
            base.PatchSelf();
            return (T)(entry.Target);
        }

        public new T PatchSelf()
        {
            base.PatchSelf();
            return (T)(entry.Target);
        }

        public T Put(T item)
        {
            base.Put<T>(item);
            return item;
        }

        public T PutFrom(T source)
        {
            ValueArray = creator.Create(source).ValueArray;
            base.PutSelf();
            return (T)(entry.Target);
        }

        public new T PutSelf()
        {
            base.PutSelf();
            return (T)(entry.Target);
        }

        public UpdaterItem[] Detect(T item)
        {
            return base.Detect<T>(item);
        }

        public new T Clone()
        {
            var clone = typeof(T).New<T>();
            var _clone = creator.Create(clone);
            _clone.ValueArray = entry.ValueArray;
            return clone;
        }

        public IProxy EntryProxy => entry;
        public IProxy PresetProxy => (IProxy)Preset;

        public new T Entry => (T)(preset.Target);
        public new T Preset => (T)((preset == null) ? preset = creator.Create(entry) : preset);

        public new T Devisor
        {
            get => (T)(entry.Target);
            set => entry.Target = value;
        }
    }
}
