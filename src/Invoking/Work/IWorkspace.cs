namespace Radical.Invoking.Work
{
    public interface IWorkspace
    {
        void Close(bool SafeClose);

        WorkAspect Allocate(int antcount = 1);

        void Run(WorkItem labor);

        void Reset(int antcount = 1);
    }
}
