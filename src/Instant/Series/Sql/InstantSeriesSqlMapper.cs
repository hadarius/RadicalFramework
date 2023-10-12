namespace Radical.Instant.Series.Sql
{
    using System;
    using System.Collections.Generic;
    using Radical.Instant.Series.Sql.Db;
    using System.Linq;
    using Radical.Series;
    using Rubrics;

    public class InstantSeriesSqlMapper
    {
        public InstantSeriesSqlMapper(
            IInstantSeries table,
            bool keysFromDeck = false,
            string[] dbTableNames = null,
            string tablePrefix = ""
        )
        {
            try
            {
                bool mixedMode = false;
                string tName = "",
                    dbtName = "",
                    prefix = tablePrefix;
                List<string> dbtNameMixList = new List<string>();
                if (dbTableNames != null)
                {
                    foreach (string dbTableName in dbTableNames)
                        if (SqlDbRegistry.Schema.Tables.Have(dbTableName))
                            dbtNameMixList.Add(dbTableName);
                    if (dbtNameMixList.Count > 0)
                        mixedMode = true;
                }
                IInstantSeries t = table;
                tName = t.FigureType.Name;
                if (!mixedMode)
                {
                    if (!SqlDbRegistry.Schema.Tables.Have(tName))
                    {
                        if (SqlDbRegistry.Schema.Tables.Have(prefix + tName))
                            dbtName = prefix + tName;
                    }
                    else
                        dbtName = tName;
                    if (!string.IsNullOrEmpty(dbtName))
                    {
                        if (!keysFromDeck)
                        {
                            Registry<int> colOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            )
                                            && !SqlDbRegistry.Schema.Tables[dbtName].PrimaryKeys
                                                .Select(pk => pk.ColumnName)
                                                .Contains(c.RubricName)
                                    )
                                    .Select(o => o.FieldId)
                            );
                            Registry<int> keyOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].PrimaryKeys
                                                .Select(pk => pk.ColumnName)
                                                .Contains(c.RubricName)
                                    )
                                    .Select(o => o.FieldId)
                            );
                            RubricSqlMapping iSqlsetMap = new RubricSqlMapping(
                                dbtName,
                                keyOrdinal,
                                colOrdinal
                            );
                            if (t.Rubrics.Mappings == null)
                                t.Rubrics.Mappings = new RubricSqlMappings();
                            t.Rubrics.Mappings.Add(iSqlsetMap.DbTableName, iSqlsetMap);
                        }
                        else
                        {
                            Registry<int> colOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            ) && !c.IsKey
                                    )
                                    .Select(o => o.FieldId)
                            );
                            Registry<int> keyOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            ) && c.IsKey
                                    )
                                    .Select(o => o.FieldId)
                            );
                            RubricSqlMapping iSqlsetMap = new RubricSqlMapping(
                                dbtName,
                                keyOrdinal,
                                colOrdinal
                            );
                            if (t.Rubrics.Mappings == null)
                                t.Rubrics.Mappings = new RubricSqlMappings();
                            t.Rubrics.Mappings.Add(iSqlsetMap.DbTableName, iSqlsetMap);
                        }
                    }
                }
                else
                {
                    if (!keysFromDeck)
                    {
                        foreach (string dbtNameMix in dbtNameMixList)
                        {
                            dbtName = dbtNameMix;
                            Registry<int> colOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            )
                                            && !SqlDbRegistry.Schema.Tables[dbtName].PrimaryKeys
                                                .Select(pk => pk.ColumnName)
                                                .Contains(c.RubricName)
                                    )
                                    .Select(o => o.FieldId)
                            );
                            Registry<int> keyOrdinal = new Registry<int>(
                                (
                                    t.Rubrics
                                        .AsValues()
                                        .Where(
                                            c =>
                                                SqlDbRegistry.Schema.Tables[dbtName].PrimaryKeys
                                                    .Select(pk => pk.ColumnName)
                                                    .Contains(c.RubricName)
                                        )
                                        .Select(o => o.FieldId)
                                )
                            );
                            if (keyOrdinal.Count == 0)
                                keyOrdinal = new Registry<int>(
                                    t.Rubrics.KeyRubrics
                                        .AsValues()
                                        .Where(
                                            c =>
                                                SqlDbRegistry.Schema.Tables[
                                                    dbtName
                                                ].Columns.Have(c.RubricName)
                                        )
                                        .Select(o => o.FieldId)
                                );
                            RubricSqlMapping iSqlsetMap = new RubricSqlMapping(
                                dbtName,
                                keyOrdinal,
                                colOrdinal
                            );
                            if (t.Rubrics.Mappings == null)
                                t.Rubrics.Mappings = new RubricSqlMappings();
                            t.Rubrics.Mappings.Add(iSqlsetMap.DbTableName, iSqlsetMap);
                        }
                    }
                    else
                    {
                        foreach (string dbtNameMix in dbtNameMixList)
                        {
                            dbtName = dbtNameMix;
                            Registry<int> colOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            ) && !c.IsKey
                                    )
                                    .Select(o => o.FieldId)
                            );
                            Registry<int> keyOrdinal = new Registry<int>(
                                t.Rubrics
                                    .AsValues()
                                    .Where(
                                        c =>
                                            SqlDbRegistry.Schema.Tables[dbtName].Columns.Have(
                                                c.RubricName
                                            ) && c.IsKey
                                    )
                                    .Select(o => o.FieldId)
                            );
                            if (keyOrdinal.Count == 0)
                                keyOrdinal = new Registry<int>(
                                    t.Rubrics.KeyRubrics
                                        .AsValues()
                                        .Where(
                                            c =>
                                                SqlDbRegistry.Schema.Tables[
                                                    dbtName
                                                ].Columns.Have(c.RubricName)
                                        )
                                        .Select(o => o.FieldId)
                                );
                            RubricSqlMapping iSqlsetMap = new RubricSqlMapping(
                                dbtName,
                                keyOrdinal,
                                colOrdinal
                            );
                            if (t.Rubrics.Mappings == null)
                                t.Rubrics.Mappings = new RubricSqlMappings();
                            t.Rubrics.Mappings.Add(iSqlsetMap.DbTableName, iSqlsetMap);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SqlMapperException(ex.ToString());
            }
            ItemsMapped = table;
        }

        public IInstantSeries ItemsMapped { get; set; }

        public class SqlMapperException : Exception
        {
            public SqlMapperException(string message) : base(message) { }
        }
    }
}
