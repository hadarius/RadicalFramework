namespace Radical.Instant.Series.Sql.Db
{
    using System.Collections.Generic;
    using System.Linq;
    using Rubrics;

    public class SqlDbTable
    {
        private SqlDbColumn[] primaryKeys;

        public SqlDbTable() { }

        public SqlDbTable(string tableName)
        {
            TableName = tableName;
        }

        public SqlDbColumns Columns { get; set; }

        public SqlDbColumn[] PrimaryKeys
        {
            get { return primaryKeys; }
            set { primaryKeys = value; }
        }

        public List<MemberRubric> GetColumnsForDataTable
        {
            get
            {
                return Columns.List
                    .Select(
                        c =>
                            new MemberRubric(
                                new FieldRubric(
                                    c.RubricType,
                                    c.ColumnName,
                                    c.DbColumnSize,
                                    c.DbOrdinal
                                )
                                {
                                    RubricSize = c.DbColumnSize
                                }
                            )
                            {
                                FieldId = c.DbOrdinal - 1,
                                IsAutoincrement = c.isAutoincrement,
                                IsDBNull = c.isDBNull,
                                IsIdentity = c.isIdentity
                            }
                    )
                    .ToList();
            }
        }

        public MemberRubric[] GetKeyForDataTable
        {
            get
            {
                return PrimaryKeys
                    .Select(
                        c =>
                            new MemberRubric(
                                new FieldRubric(
                                    c.RubricType,
                                    c.ColumnName,
                                    c.DbColumnSize,
                                    c.DbOrdinal
                                )
                                {
                                    RubricSize = c.DbColumnSize
                                }
                            )
                            {
                                FieldId = c.DbOrdinal - 1,
                                IsAutoincrement = c.isAutoincrement,
                                IsDBNull = c.isDBNull,
                                IsIdentity = c.isIdentity
                            }
                    )
                    .ToArray();
            }
        }

        public string TableName { get; set; }
    }
}
