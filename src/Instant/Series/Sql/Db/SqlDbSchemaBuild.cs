namespace Radical.Instant.Series.Sql.Db
{
    using Microsoft.Data.SqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public enum BuildDbSchemaType
    {
        Schema,
        Temp
    }

    public class SqlDbSchemaBuild
    {
        private SqlConnection sqlcn;

        public SqlDbSchemaBuild(SqlConnection _sqlcn)
        {
            sqlcn = _sqlcn;
        }

        public void SchemaPrepare(BuildDbSchemaType buildtype = BuildDbSchemaType.Schema)
        {
            SqlDbSchema dbs = new SqlDbSchema(sqlcn);
            bool wasOpen = false;
            if (sqlcn.State == ConnectionState.Open)
                wasOpen = true;
            if (!wasOpen)
                sqlcn.Open();
            IEnumerable<DataRow> table = sqlcn
                .GetSchema("Tables")
                .Rows.Cast<DataRow>()
                .AsEnumerable()
                .AsQueryable();
            IEnumerable<DataRow> columns = sqlcn
                .GetSchema("Columns")
                .Rows.Cast<DataRow>()
                .AsEnumerable()
                .AsQueryable();
            IEnumerable<DataRow> index = sqlcn
                .GetSchema("IndexColumns")
                .Rows.Cast<DataRow>()
                .AsEnumerable()
                .AsQueryable();
            List<SqlDbTable> dbTables = table
                .Select(
                    t =>
                        new SqlDbTable()
                        {
                            TableName = t["TABLE_NAME"].ToString(),
                            Columns = new SqlDbColumns()
                            {
                                List = columns
                                    .Where(c => t["TABLE_NAME"].Equals(c["TABLE_NAME"]))
                                    .Select(
                                        k =>
                                            new SqlDbColumn
                                            {
                                                ColumnName = k["COLUMN_NAME"].ToString(),
                                                RubricType = SqlDbNetType.SqlTypeToNet(
                                                    k["DATA_TYPE"].ToString()
                                                ),
                                                MaxLength =
                                                    (k["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                                                        ? Convert.ToInt32(
                                                            k["CHARACTER_MAXIMUM_LENGTH"]
                                                        )
                                                        : -1,
                                                isDBNull =
                                                    (k["IS_NULLABLE"].ToString() == "YES")
                                                        ? true
                                                        : false,
                                                DbOrdinal = Convert.ToInt32(k["ORDINAL_POSITION"])
                                            }
                                    )
                                    .ToList()
                            },
                            PrimaryKeys = index
                                .Where(i => t["TABLE_NAME"].Equals(i["table_name"]))
                                .Where(
                                    it =>
                                        columns
                                            .Where(
                                                c =>
                                                    c["TABLE_NAME"].Equals(it["table_name"])
                                                    && c["COLUMN_NAME"].Equals(it["column_name"])
                                            )
                                            .Any()
                                )
                                .Select(
                                    k =>
                                        new SqlDbColumn()
                                        {
                                            ColumnName = k["column_name"].ToString(),
                                            isIdentity =
                                                (k["KeyType"].ToString() == "56") ? true : false,
                                            isKey = true,
                                            DbOrdinal = Convert.ToInt32(k["ordinal_position"]),
                                            RubricType = SqlDbNetType.SqlTypeToNet(
                                                columns
                                                    .Where(
                                                        c =>
                                                            c["TABLE_NAME"].Equals(k["table_name"])
                                                            && c["COLUMN_NAME"].Equals(
                                                                k["column_name"]
                                                            )
                                                    )
                                                    .First()["DATA_TYPE"].ToString()
                                            )
                                        }
                                )
                                .ToArray()
                        }
                )
                .ToList();

            dbs.Tables.AddRange(dbTables.ToList());
            if (buildtype == BuildDbSchemaType.Schema)
                SqlDbRegistry.Schema = dbs;
            else
                SqlDbRegistry.Temp = dbs;
        }
    }
}
