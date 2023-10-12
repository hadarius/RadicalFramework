namespace Radical.Instant.Series
{
    using Ethernet.Transit;
    using Math;
    using Querying;
    using System.Linq;
    using Radical.Series;
    using Rubrics;
    using Instant.Proxies;

    public interface IInstantSeries : ISeries<IInstant>, IInstant, ITransitFormatter
    {
        IInstantCreator Creator { get; set; }

        bool Prime { get; set; }

        new IInstant this[int index] { get; set; }

        object this[int index, string propertyName] { get; set; }

        object this[int index, int fieldId] { get; set; }

        IRubrics Rubrics { get; set; }

        IRubrics KeyRubrics { get; set; }

        IInstant NewInstant();

        IProxy NewProxy();

        Type FigureType { get; set; }

        int FigureSize { get; set; }

        Type Type { get; set; }

        IQueryable<IInstant> View { get; set; }

        IInstant Total { get; set; }

        InstantSeriesFilter Filter { get; set; }

        InstantSeriesSort Sort { get; set; }

        Func<IInstant, bool> Predicate { get; set; }
         
        InstantSeriesAggregate Aggregate { get; set; }

        ISeries<IInstantSeriesMath> Computations { get; set; }
    }
}
