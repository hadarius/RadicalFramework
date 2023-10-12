namespace Radical.Instant.Series.Sql.Db
{
    using Microsoft.Data.SqlClient;
    using System.Collections.Generic;

    public class SqlDbSchema
    {
        public SqlDbSchema(SqlConnection sqlDbConnection)
        {
            Tables = new SqlDbTables();
            Options = new SqlDbOptions(sqlDbConnection.ConnectionString);
        }

        public SqlDbSchema(string dbConnectionString)
        {
            Tables = new SqlDbTables();
            Options = new SqlDbOptions(dbConnectionString);
            SqlDbConnection = new SqlConnection(dbConnectionString);
        }

        public SqlDbTables Tables { get; set; }

        public SqlDbOptions Options { get; set; }

        public string Name { get; set; }

        public List<SqlDbTable> TableList
        {
            get { return Tables.List; }
            set { Tables.List = value; }
        }

        public SqlConnection SqlDbConnection { get; set; }
    }
}
