namespace Radical.Invoking.Work
{
    using Notes;

    public interface IWorker
    {
        object GetInput();
        void SetInput(object value);

        object GetOutput();
        void SetOutput(object value);

        WorkNoteEvokers Evokers { get; set; }

        string Name { get; set; }

        IInvoker Process { get; set; }

        WorkAspect FlowTo(WorkItem Recipient);

        WorkAspect FlowTo(WorkItem Recipient, params WorkItem[] RelationWorks);

        WorkAspect FlowTo(string RecipientName);

        WorkAspect FlowTo(string RecipientName, params string[] RelationNames);

        WorkAspect FlowFrom(WorkItem Recipient);

        WorkAspect FlowFrom(WorkItem Recipient, params WorkItem[] RelationWorks);

        WorkAspect FlowFrom(string RecipientName);

        WorkAspect FlowFrom(string RecipientName, params string[] RelationNames);
    }
}
