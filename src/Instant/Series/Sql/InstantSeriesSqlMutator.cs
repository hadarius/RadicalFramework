namespace Radical.Instant.Series.Sql
{
    using Radical.Series;
    using Db;

    public class InstantSeriesSqlMutator
    {
        private InstantSeriesSqlDb sqaf;

        public InstantSeriesSqlMutator() { }

        public InstantSeriesSqlMutator(InstantSeriesSqlDb insql)
        {
            sqaf = insql;
        }

        public ISeries<ISeries<IInstant>> Delete(string SqlConnectString, IInstantSeries items)
        {
            try
            {
                if (sqaf == null)
                    sqaf = new InstantSeriesSqlDb(SqlConnectString);
                try
                {
                    bool buildmap = true;
                    if (items.Count > 0)
                    {
                        SqlDbBulkPrepareType prepareType = SqlDbBulkPrepareType.Drop;
                        return sqaf.Delete(items, true, buildmap, prepareType);
                    }
                    return null;
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ISeries<ISeries<IInstant>> Set(string SqlConnectString, IInstantSeries items, bool Renew)
        {
            try
            {
                if (sqaf == null)
                    sqaf = new InstantSeriesSqlDb(SqlConnectString);
                try
                {
                    bool buildmap = true;
                    if (items.Count > 0)
                    {
                        SqlDbBulkPrepareType prepareType = SqlDbBulkPrepareType.Drop;

                        if (Renew)
                            prepareType = SqlDbBulkPrepareType.Trunc;

                        var ds = sqaf.Update(items, true, buildmap, true, null, prepareType);
                        if (ds != null)
                        {
                            IInstantSeries im = (IInstantSeries)Instances.New(items.GetType());
                            im.Rubrics = items.Rubrics;
                            im.FigureType = items.FigureType;
                            im.FigureSize = items.FigureSize;
                            im.Add(ds["Failed"].AsValues());
                            return sqaf.Insert(im, true, false, prepareType);
                        }
                        else
                            return null;
                    }
                    return null;
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
