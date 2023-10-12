namespace Radical.Instant.Series.Math;

using System.Collections.Generic;
using System.Linq;
using Instant.Rubrics;

public static class InstantSeriesMathExtensions
{
    public static IInstantSeries Compute(this IInstantSeries series)
    {
        series.Computations.Select(c => c.Compute()).ToArray();
        return series;
    }

    public static IInstantSeries Compute(this IInstantSeries series, IList<MemberRubric> rubrics)
    {
        IInstantSeriesMath[] ic = rubrics
            .Select(
                r =>
                    series.Computations
                        .Where(c => ((InstantSeriesMath)c).ContainsFirst(r))
                        .FirstOrDefault()
            )
            .Where(c => c != null)
            .ToArray();
        ic.Select(c => c.Compute()).ToArray();
        return series;
    }

    public static IInstantSeries Compute(this IInstantSeries series, IList<string> rubricNames)
    {
        IInstantSeriesMath[] ic = rubricNames
            .Select(
                r =>
                    series.Computations
                        .Where(c => ((InstantSeriesMath)c).ContainsFirst(r))
                        .FirstOrDefault()
            )
            .Where(c => c != null)
            .ToArray();
        ic.Select(c => c.Compute()).ToArray();
        return series;
    }

    public static IInstantSeries Compute(this IInstantSeries series, MemberRubric rubric)
    {
        IInstantSeriesMath ic = series.Computations
            .Where(c => ((InstantSeriesMath)c).ContainsFirst(rubric))
            .FirstOrDefault();
        if (ic != null)
            ic.Compute();
        return series;
    }

    public static IInstantSeries ComputeParallel(this IInstantSeries series)
    {
        series.Computations.AsParallel().Select(c => c.Compute()).ToArray();
        return series;
    }

    public static IInstantSeries ComputeParallel(this IInstantSeries series, IList<MemberRubric> rubrics)
    {
        IInstantSeriesMath[] ic = rubrics
            .Select(
                r =>
                    series.Computations
                        .Where(c => ((InstantSeriesMath)c).ContainsFirst(r))
                        .FirstOrDefault()
            )
            .Where(c => c != null)
            .ToArray();
        ic.AsParallel().Select(c => c.Compute()).ToArray();
        return series;
    }

    public static IInstantSeries ComputeParallel(this IInstantSeries series, IList<string> rubricNames)
    {
        IInstantSeriesMath[] ic = rubricNames
            .Select(
                r =>
                    series.Computations
                        .Where(c => ((InstantSeriesMath)c).ContainsFirst(r))
                        .FirstOrDefault()
            )
            .Where(c => c != null)
            .ToArray();
        ic.AsParallel().Select(c => c.Compute()).ToArray();
        return series;
    }
}
