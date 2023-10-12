namespace Radical.Instant.Series.Proxies
{
    public interface IInstantSeriesProxy : IInstantSeries
    {
        IInstantSeries Series { get; set; }

        new IInstantCreator Creator { get; set; }

        IInstantSeries Proxies { get; set; }
    }
}
