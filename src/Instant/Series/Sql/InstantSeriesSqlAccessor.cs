namespace Radical.Instant.Series.Sql
{
    using Microsoft.Data.SqlClient;
    using System.Collections.Generic;
    using System.Data;
    using Radical.Series;
    using Db;

    public class InstantSeriesSqlAccessor
    {
        public InstantSeriesSqlAccessor() { }

        public IInstantSeries Get(
            string sqlConnectString,
            string sqlQry,
            string tableName,
            ISeries<string> keyNames
        )
        {
            try
            {
                if (SqlDbRegistry.Schema == null || SqlDbRegistry.Schema.TableList.Count == 0)
                {
                    InstantSeriesSqlDb sqb = new InstantSeriesSqlDb(sqlConnectString);
                }
                InstantSeriesSqlExecutor sqa = new InstantSeriesSqlExecutor(sqlConnectString);

                try
                {
                    return sqa.ExecuteSelect(sqlQry, tableName, keyNames);
                }
                catch (Exception ex)
                {
                    throw new SqlException(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetSqlDataTable(object parameters)
        {
            try
            {
                Dictionary<string, object> param = new Dictionary<string, object>(
                    (Dictionary<string, object>)parameters
                );
                string sqlQry = param["SqlQuery"].ToString();
                string sqlConnectString = param["ConnectionString"].ToString();

                DataTable Table = new DataTable();
                SqlConnection sqlcn = new SqlConnection(sqlConnectString);
                sqlcn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQry, sqlcn);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetSqlDataTable(SqlCommand cmd)
        {
            try
            {
                DataTable Table = new DataTable();
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetSqlDataTable(string qry, SqlConnection cn)
        {
            try
            {
                DataTable Table = new DataTable();
                if (cn.State == ConnectionState.Closed)
                    cn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(qry, cn);
                adapter.Fill(Table);
                return Table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
