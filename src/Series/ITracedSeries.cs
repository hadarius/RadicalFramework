namespace Radical.Series
{
    using Invoking;
    using Instant.Updating;

    public interface ITracedSeries
    {
        IUpdater Updater { get; }

        IInvoker NoticeChange { get; }

        IInvoker NoticeChanging { get; }
    }
}
