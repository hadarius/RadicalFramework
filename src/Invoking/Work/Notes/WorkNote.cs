namespace Radical.Invoking.Work.Notes
{
    using Uniques;

    public class WorkNote : IUnique
    {
        public object[] Parameters;
        public WorkNoteBox SenderBox;

        public WorkNote(
            WorkItem sender,
            WorkItem recipient,
            WorkNoteEvoker Out,
            WorkNoteEvokers In,
            params object[] Params
        )
        {
            Parameters = Params;

            if (recipient != null)
            {
                Recipient = recipient;
                RecipientName = Recipient.Worker.Name;
            }

            Sender = sender;
            SenderName = Sender.Worker.Name;

            if (Out != null)
                EvokerOut = Out;

            if (In != null)
                EvokersIn = In;
        }

        public WorkNote(string sender, params object[] Params) : this(sender, null, null, null, Params)
        { }

        public WorkNote(
            string sender,
            string recipient,
            WorkNoteEvoker Out,
            WorkNoteEvokers In,
            params object[] Params
        )
        {
            SenderName = sender;
            Parameters = Params;

            if (recipient != null)
                RecipientName = recipient;

            if (Out != null)
                EvokerOut = Out;

            if (In != null)
                EvokersIn = In;
        }

        public WorkNote(string sender, string recipient, WorkNoteEvoker Out, params object[] Params)
            : this(sender, recipient, Out, null, Params) { }

        public WorkNote(string sender, string recipient, params object[] Params)
            : this(sender, recipient, null, null, Params) { }

        public IUnique Empty => new Uscn();

        public WorkNoteEvoker EvokerOut { get; set; }

        public WorkNoteEvokers EvokersIn { get; set; }

        public WorkItem Recipient { get; set; }

        public string RecipientName { get; set; }

        public WorkItem Sender { get; set; }

        public string SenderName { get; set; }

        public ulong Key
        {
            get => Sender.Key;
            set => Sender.Key = value;
        }

        public ulong TypeKey
        {
            get => ((IUnique)Sender).TypeKey;
            set => ((IUnique)Sender).TypeKey = value;
        }

        public int CompareTo(IUnique other)
        {
            return Sender.CompareTo(other);
        }

        public bool Equals(IUnique other)
        {
            return Sender.Equals(other);
        }

        public byte[] GetBytes()
        {
            return Sender.GetBytes();
        }

        public byte[] GetKeyBytes()
        {
            return Sender.GetKeyBytes();
        }
    }
}
