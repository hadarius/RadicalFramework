namespace Radical.Instant.Series.Math;

using Uniques;

public interface IInstantSeriesMath : IUnique
{
    IInstantSeries Compute();
}
