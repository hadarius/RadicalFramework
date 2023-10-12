namespace Radical.Invoking.Work
{
    using Series;

    public class WorkMethods : Registry<IInvoker>
    {
        public override ISeriesItem<IInvoker>[] EmptyVector(int size)
        {
            return new WorkMethod[size];
        }

        public override ISeriesItem<IInvoker> EmptyItem()
        {
            return new WorkMethod();
        }

        public override ISeriesItem<IInvoker>[] EmptyTable(int size)
        {
            return new WorkMethod[size];
        }

        public override ISeriesItem<IInvoker> NewItem(IInvoker value)
        {
            return new WorkMethod(value);
        }

        public override ISeriesItem<IInvoker> NewItem(object key, IInvoker value)
        {
            return new WorkMethod(key, value);
        }

        public override ISeriesItem<IInvoker> NewItem(ulong key, IInvoker value)
        {
            return new WorkMethod(key, value);
        }
    }
}
