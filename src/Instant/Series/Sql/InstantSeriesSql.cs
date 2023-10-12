using Radical.Series;
using Radical.Series.Base;

namespace Radical.Instant.Series.Sql;

using Instant.Updating;

public class InstantSeriesSql<T> : BaseRegistry<IUpdater<T>>
{
    public InstantSeriesSql() { }

    public InstantSeriesSql(InstantSeriesSqlContext sqlcontext)
    {
        Context = sqlcontext;
    }
 
    public IInstantSeries Series { get; private set; }

    public InstantSeriesSqlContext Context { get; }
}

public class InstantSeriesSql : BaseRegistry<IUpdater>
{
    public InstantSeriesSql() { }

    public InstantSeriesSql(InstantSeriesSqlContext sqlcontext)
    {
        Context = sqlcontext;
    }

    public IInstantSeries Series { get; private set; }

    public InstantSeriesSqlContext Context { get; }
}
