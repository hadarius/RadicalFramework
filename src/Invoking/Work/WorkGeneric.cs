namespace Radical.Invoking.Work
{
    public class Work<T> : WorkItem
    {
        public Work(Func<T, string> method) : base(new Invoker<T>(method)) { }
    }
}
