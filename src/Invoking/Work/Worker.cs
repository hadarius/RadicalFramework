namespace Radical.Invoking.Work
{
    using Series;
    using Uniques;
    using Notes;

    public class Worker : IUnique, IWorker
    {
        private readonly Registry<object> input = new Registry<object>(true);
        private readonly Registry<object> output = new Registry<object>(true);
        private Uscn SerialCode;

        private Worker() { }

        public Worker(string Name, IInvoker Method) : this()
        {
            Process = Method;
            this.Name = Name;
            ulong seed = Unique.NewKey;
            SerialCode = new Uscn((Process.Key).UniqueKey(seed), seed);
        }

        public IUnique Empty => new Uscn();

        public WorkNoteEvokers Evokers { get; set; } = new WorkNoteEvokers();

        public object GetInput()
        {
            object entry;
            input.TryDequeue(out entry);
            return entry;
        }

        public void SetInput(object value)
        {
            input.Enqueue(value);
        }

        public object GetOutput()
        {
            object entry;
            output.TryDequeue(out entry);
            return entry;
        }

        public void SetOutput(object value)
        {
            output.Enqueue(value);
        }

        public WorkItem Work { get; set; }

        public string Name { get; set; }

        public ulong Key
        {
            get => SerialCode.Key;
            set => SerialCode.Key = value;
        }

        public ulong TypeKey
        {
            get => SerialCode.TypeKey;
            set => SerialCode.TypeKey = value;
        }

        public IInvoker Process { get; set; }

        public WorkAspect FlowTo<T>()
        {
            return Work.FlowTo<T>();
        }

        public WorkAspect FlowTo(WorkItem recipient)
        {
            Evokers.Add(new WorkNoteEvoker(Work, recipient, Work));
            return Work.Aspect;
        }

        public WorkAspect FlowTo(WorkItem Recipient, params WorkItem[] RelationWorks)
        {
            Evokers.Add(new WorkNoteEvoker(Work, Recipient, RelationWorks));
            return Work.Aspect;
        }

        public WorkAspect FlowTo(string RecipientName)
        {
            Evokers.Add(new WorkNoteEvoker(Work, RecipientName, Name));
            return Work.Aspect;
        }

        public WorkAspect FlowTo(string RecipientName, params string[] RelationNames)
        {
            Evokers.Add(new WorkNoteEvoker(Work, RecipientName, RelationNames));
            return Work.Aspect;
        }

        public WorkAspect FlowFrom<T>()
        {
            return Work.FlowFrom<T>();
        }

        public WorkAspect FlowFrom(WorkItem sender)
        {
            Work.FlowFrom(sender);
            return Work.Aspect;
        }

        public WorkAspect FlowFrom(WorkItem Sender, params WorkItem[] RelationWorks)
        {
            Work.FlowFrom(Sender, RelationWorks);
            return Work.Aspect;
        }

        public WorkAspect FlowFrom(string SenderName)
        {
            Work.FlowFrom(SenderName);
            return Work.Aspect;
        }

        public WorkAspect FlowFrom(string SenderName, params string[] RelationNames)
        {
            Work.FlowFrom(SenderName, RelationNames);
            return Work.Aspect;
        }

        public int CompareTo(IUnique other)
        {
            return SerialCode.CompareTo(other);
        }

        public bool Equals(IUnique other)
        {
            return SerialCode.Equals(other);
        }

        public byte[] GetBytes()
        {
            return SerialCode.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return SerialCode.GetKeyBytes();
        }
    }
}
