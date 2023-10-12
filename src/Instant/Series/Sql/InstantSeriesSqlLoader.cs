namespace Radical.Instant.Series.Sql
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Radical.Instant.Series.Sql.Db;
    using Radical.Series;
    using Radical.Uniques;
    using Rubrics;

    public class InstantSeriesSqlLoader<T> : IInstantSeriesSqlLoader<T> where T : class
    {
        private IDataReader dr;

        public InstantSeriesSqlLoader(IDataReader _dr)
        {
            dr = _dr;
        }

        public IInstantSeries LoadSchema(
            DataTable schema,
            ISeries<MemberRubric> operColumns,
            bool insAndDel = false
        )
        {
            List<MemberRubric> columns = new List<MemberRubric>(
                schema.Rows
                    .Cast<DataRow>()
                    .AsEnumerable()
                    .AsQueryable()
                    .Select(
                        c =>
                            new MemberRubric(
                                new FieldRubric(
                                    Type.GetType(c["DataType"].ToString()),
                                    c["ColumnName"].ToString(),
                                    Convert.ToInt32(c["ColumnSize"]),
                                    Convert.ToInt32(c["ColumnOrdinal"])
                                )
                                {
                                    RubricSize = Convert.ToInt32(c["ColumnSize"])
                                }
                            )
                            {
                                FieldId = Convert.ToInt32(c["ColumnOrdinal"]),
                                IsIdentity = Convert.ToBoolean(c["IsIdentity"]),
                                IsAutoincrement = Convert.ToBoolean(c["IsAutoincrement"]),
                                IsDBNull = Convert.ToBoolean(c["AllowDBNull"])
                            }
                    )
                    .ToList()
            );

            List<MemberRubric> _columns = new List<MemberRubric>();

            if (insAndDel)
                for (int i = 0; i < (int)(columns.Count / 2); i++)
                    _columns.Add(columns[i]);
            else
                _columns.AddRange(columns);

            InstantCreator rt = new InstantCreator(_columns.ToArray(), "SchemaFigure");
            InstantSeriesCreator tab = new InstantSeriesCreator(rt, "Schema");
            IInstantSeries catalog = tab.Combine();

            List<SqlDbTable> dbtabs = SqlDbRegistry.Schema.TableList;
            MemberRubric[] pKeys = columns
                .Where(
                    c =>
                        dbtabs
                            .SelectMany(t => t.GetKeyForDataTable.Select(d => d.RubricName))
                            .Contains(c.RubricName)
                        && operColumns.Select(o => o.RubricName).Contains(c.RubricName)
                )
                .ToArray();
            if (pKeys.Length > 0)
                catalog.Rubrics.KeyRubrics = new MemberRubrics(pKeys);
            catalog.Rubrics.Update();
            return catalog;
        }

        public ISeries<ISeries<IInstant>> LoadDelete(IInstantSeries toDeleteItems)
        {
            IInstantSeries catalog = toDeleteItems;
            ISeries<IInstant> deletedList = new Registry<IInstant>();
            ISeries<IInstant> brokenList = new Registry<IInstant>();

            int i = 0;
            do
            {
                int columnsCount = catalog.Rubrics.Count;

                if (i == 0 && catalog.Rubrics.Count == 0)
                {
                    IInstantSeries tab = LoadSchema(dr.GetSchemaTable(), catalog.Rubrics.KeyRubrics);
                    catalog = tab;
                    columnsCount = catalog.Rubrics.Count;
                }
                object[] itemArray = new object[columnsCount];
                int[] keyIndexes = catalog.Rubrics.KeyRubrics.Ordinals;
                while (dr.Read())
                {
                    if ((columnsCount - 1) != dr.FieldCount)
                    {
                        IInstantSeries tab = LoadSchema(dr.GetSchemaTable(), catalog.Rubrics.KeyRubrics);
                        catalog = tab;
                        columnsCount = catalog.Rubrics.Count;
                        itemArray = new object[columnsCount];
                        keyIndexes = catalog.Rubrics.KeyRubrics.Ordinals;
                    }

                    dr.GetValues(itemArray);

                    IInstant row = catalog.NewInstant();

                    row.ValueArray = itemArray
                        .Select(
                            (a, y) => itemArray[y] = (a == DBNull.Value) ? a.GetType().Default() : a
                        )
                        .ToArray();

                    deletedList.Add(row);
                }

                foreach (IInstant ir in toDeleteItems)
                    if (!deletedList.ContainsKey(ir))
                        brokenList.Add(ir);
            } while (dr.NextResult());

            ISeries<ISeries<IInstant>> iSet = new Registry<ISeries<IInstant>>();

            iSet.Add("Failed", brokenList);

            iSet.Add("Deleted", deletedList);

            return iSet;
        }

        public IInstantSeries LoadSelect(string tableName, ISeries<string> keyNames = null)
        {
            DataTable schema = dr.GetSchemaTable();
            List<MemberRubric> columns = new List<MemberRubric>(
                schema.Rows
                    .Cast<DataRow>()
                    .AsEnumerable()
                    .AsQueryable()
                    .Where(n => n["ColumnName"].ToString() != "Code")
                    .Select(
                        c =>
                            new MemberRubric(
                                new FieldRubric(
                                    Type.GetType(c["DataType"].ToString()),
                                    c["ColumnName"].ToString(),
                                    Convert.ToInt32(c["ColumnSize"]),
                                    Convert.ToInt32(c["ColumnOrdinal"])
                                )
                                {
                                    RubricSize = Convert.ToInt32(c["ColumnSize"])
                                }
                            )
                            {
                                FieldId = Convert.ToInt32(c["ColumnOrdinal"]),
                                IsIdentity = Convert.ToBoolean(c["IsIdentity"]),
                                IsAutoincrement = Convert.ToBoolean(c["IsAutoincrement"]),
                                IsDBNull = Convert.ToBoolean(c["AllowDBNull"]),
                            }
                    )
                    .ToList()
            );

            bool takeDbKeys = false;
            if (keyNames != null)
                if (keyNames.Count > 0)
                    foreach (var k in keyNames)
                    {
                        columns.Where(c => c.Name == k).Select(ck => ck.IsKey = true).ToArray();
                    }
                else
                    takeDbKeys = true;
            else
                takeDbKeys = true;

            if (takeDbKeys && SqlDbRegistry.Schema != null && SqlDbRegistry.Schema.TableList.Count > 0)
            {
                List<SqlDbTable> dbtabs = SqlDbRegistry.Schema.TableList;
                MemberRubric[] pKeys = columns
                    .Where(
                        c =>
                            dbtabs
                                .SelectMany(t => t.GetKeyForDataTable.Select(d => d.RubricName))
                                .Contains(c.RubricName)
                    )
                    .ToArray();

                if (pKeys.Length > 0)
                {
                    pKeys.Select(pk => pk.IsKey = true);
                }
            }

            InstantCreator rt = new InstantCreator(columns.ToArray(), tableName);
            InstantSeriesCreator catalog = new InstantSeriesCreator(rt, tableName + "_Figures");
            IInstantSeries tab = catalog.Combine();

            if (dr.Read())
            {
                int columnsCount = dr.FieldCount;
                object[] itemArray = new object[columnsCount];
                int[] keyOrder = tab.Rubrics.KeyRubrics.Ordinals;

                do
                {
                    IInstant figure = tab.NewInstant();

                    dr.GetValues(itemArray);

                    figure.ValueArray = itemArray
                        .Select(
                            (a, y) => itemArray[y] = (a == DBNull.Value) ? a.GetType().Default() : a
                        )
                        .ToArray();

                    figure.Key = keyOrder.Select(i => itemArray[i]).ToArray().UniqueKey64();

                    tab.Put(figure);
                } while (dr.Read());
                itemArray = null;
            }
            dr.Dispose();
            return tab;
        }

        public ISeries<ISeries<IInstant>> LoadInsert(IInstantSeries toInsertItems)
        {
            IInstantSeries catalog = toInsertItems;
            ISeries<IInstant> insertedList = new Registry<IInstant>();
            ISeries<IInstant> brokenList = new Registry<IInstant>();

            int i = 0;
            do
            {
                int columnsCount = catalog.Rubrics.Count;

                if (i == 0 && catalog.Rubrics.Count == 0)
                {
                    IInstantSeries tab = LoadSchema(dr.GetSchemaTable(), catalog.Rubrics.KeyRubrics);
                    catalog = tab;
                    columnsCount = catalog.Rubrics.Count;
                }
                object[] itemArray = new object[columnsCount];
                int[] keyIndexes = catalog.Rubrics.KeyRubrics.Ordinals;
                while (dr.Read())
                {
                    if ((columnsCount - 1) != dr.FieldCount)
                    {
                        IInstantSeries tab = LoadSchema(dr.GetSchemaTable(), catalog.Rubrics.KeyRubrics);
                        catalog = tab;
                        columnsCount = catalog.Rubrics.Count;
                        itemArray = new object[columnsCount];
                        keyIndexes = catalog.Rubrics.KeyRubrics
                            .AsValues()
                            .Select(k => k.FieldId)
                            .ToArray();
                    }

                    dr.GetValues(itemArray);

                    IInstant row = catalog.NewInstant();

                    row.ValueArray = itemArray
                        .Select(
                            (a, y) => itemArray[y] = (a == DBNull.Value) ? a.GetType().Default() : a
                        )
                        .ToArray();

                    insertedList.Add(row);
                }

                foreach (IInstant ir in toInsertItems)
                    if (!insertedList.ContainsKey(ir))
                        brokenList.Add(ir);
            } while (dr.NextResult());

            ISeries<ISeries<IInstant>> iSet = new Registry<ISeries<IInstant>>();

            iSet.Add("Failed", brokenList);

            iSet.Add("Inserted", insertedList);

            return iSet;
        }

        public ISeries<ISeries<IInstant>> LoadUpdate(IInstantSeries toUpdateItems)
        {
            IInstantSeries catalog = toUpdateItems;
            ISeries<IInstant> updatedList = new Registry<IInstant>();
            ISeries<IInstant> toInsertList = new Registry<IInstant>();

            int i = 0;
            do
            {
                int columnsCount = catalog.Rubrics != null ? catalog.Rubrics.Count : 0;

                if (i == 0 && columnsCount == 0)
                {
                    IInstantSeries tab = LoadSchema(
                        dr.GetSchemaTable(),
                        catalog.Rubrics.KeyRubrics,
                        true
                    );
                    catalog = tab;
                    columnsCount = catalog.Rubrics.Count;
                }
                object[] itemArray = new object[columnsCount];
                int[] keyOrder = catalog.Rubrics.KeyRubrics.Ordinals;
                while (dr.Read())
                {
                    if ((columnsCount - 1) != (int)(dr.FieldCount / 2))
                    {
                        IInstantSeries tab = LoadSchema(
                            dr.GetSchemaTable(),
                            catalog.Rubrics.KeyRubrics,
                            true
                        );
                        catalog = tab;
                        columnsCount = catalog.Rubrics.Count;
                        itemArray = new object[columnsCount];
                        keyOrder = catalog.Rubrics.KeyRubrics
                            .AsValues()
                            .Select(k => k.FieldId)
                            .ToArray();
                    }

                    dr.GetValues(itemArray);

                    IInstant row = catalog.NewInstant();

                    row.ValueArray = itemArray
                        .Select(
                            (a, y) => itemArray[y] = (a == DBNull.Value) ? a.GetType().Default() : a
                        )
                        .ToArray();

                    updatedList.Add(row);
                }

                foreach (IInstant ir in toUpdateItems)
                    if (!updatedList.ContainsKey(ir))
                        toInsertList.Add(ir);
            } while (dr.NextResult());

            ISeries<ISeries<IInstant>> iSet = new Registry<ISeries<IInstant>>();

            iSet.Add("Failed", toInsertList);

            iSet.Add("Updated", updatedList);

            return iSet;
        }
    }
}
