namespace Radical.Instant.Series.Sql
{
    using Radical.Series;

    public interface IInstantSeriesSqlLoader<T> where T : class
    {
        ISeries<ISeries<IInstant>> LoadDelete(IInstantSeries toInsertItems);

        IInstantSeries LoadSelect(string tableName, ISeries<string> keyNames = null);

        ISeries<ISeries<IInstant>> LoadInsert(IInstantSeries toInsertItems);

        ISeries<ISeries<IInstant>> LoadUpdate(IInstantSeries toUpdateItems);
    }
}
