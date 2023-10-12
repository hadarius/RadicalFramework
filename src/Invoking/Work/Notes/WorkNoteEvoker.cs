namespace Radical.Invoking.Work.Notes
{
    using System.Linq;
    using Series;
    using Uniques;

    public class WorkNoteEvoker : Catalog<WorkItem>, IUnique
    {
        public ISeries<WorkItem> RelatedWorks = new Catalog<WorkItem>();
        public ISeries<string> RelatedWorkNames = new Catalog<string>();

        public WorkNoteEvoker(WorkItem sender, WorkItem recipient, params WorkItem[] relayWorks)
        {
            Sender = sender;
            SenderName = sender.Worker.Name;
            Recipient = recipient;
            RecipientName = recipient.Worker.Name;
            Key = SenderName.UniqueKey(RecipientName.UniqueKey());
            TypeKey = RecipientName.UniqueKey();
            RelatedWorks.Add(relayWorks);
            RelatedWorkNames.Add(RelatedWorks.Select(rn => rn.Worker.Name));
        }

        public WorkNoteEvoker(WorkItem sender, WorkItem recipient, params string[] relayNames)
        {
            Sender = sender;
            SenderName = sender.Name;
            Recipient = recipient;
            RecipientName = recipient.Name;
            Key = SenderName.UniqueKey(RecipientName.UniqueKey());
            TypeKey = RecipientName.UniqueKey();
            RelatedWorkNames.Add(relayNames);
            var namekeys = relayNames.ForEach(s => s.UniqueKey());
            RelatedWorks.Add(
                Sender.Case
                    .AsValues()
                    .Where(m => m.Any(k => namekeys.Contains(k.Key)))
                    .SelectMany(os => os.AsValues())
                    .ToList()
            );
        }

        public WorkNoteEvoker(WorkItem sender, string recipientName, params WorkItem[] relayWorks)
        {
            Sender = sender;
            SenderName = sender.Name;
            RecipientName = recipientName;
            Key = SenderName.UniqueKey(RecipientName.UniqueKey());
            TypeKey = RecipientName.UniqueKey();
            var rcpts = Sender.Case
                .AsValues()
                .Where(m => m.ContainsKey(recipientName))
                .SelectMany(os => os.AsValues())
                .ToArray();
            Recipient = rcpts.FirstOrDefault();
            RelatedWorks.Add(relayWorks);
            RelatedWorkNames.Add(RelatedWorks.Select(rn => rn.Worker.Name));
        }

        public WorkNoteEvoker(WorkItem sender, string recipientName, params string[] relayNames)
        {
            Sender = sender;
            SenderName = sender.Worker.Name;
            var rcpts = Sender.Case
                .AsValues()
                .Where(m => m.ContainsKey(recipientName))
                .SelectMany(os => os.AsValues())
                .ToArray();
            Recipient = rcpts.FirstOrDefault();
            RecipientName = recipientName;
            Key = SenderName.UniqueKey(RecipientName.UniqueKey());
            TypeKey = RecipientName.UniqueKey();
            RelatedWorkNames.Add(relayNames);
            var namekeys = relayNames.ForEach(s => s.UniqueKey());
            RelatedWorks.Add(
                Sender.Case
                    .AsValues()
                    .Where(m => m.Any(k => namekeys.Contains(k.Key)))
                    .SelectMany(os => os.AsValues())
                    .ToList()
            );
        }

        public IUnique Empty => new Usid();

        public string EvokerName { get; set; }

        public EvokerType EvokerType { get; set; }

        public WorkItem Recipient { get; set; }

        public string RecipientName { get; set; }

        public WorkItem Sender { get; set; }

        public string SenderName { get; set; }
    }
}
