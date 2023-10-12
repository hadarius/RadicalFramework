namespace Radical.Instant.Series.Sql
{
    using Microsoft.Data.SqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Radical.Instant.Series.Sql.Db;
    using System.Linq;
    using Radical.Series;
    using Db.Command;
    using Rubrics;

    public class InstantSeriesSqlExecutor
    {
        private SqlCommand _cmd;
        private SqlConnection _cn;

        public InstantSeriesSqlExecutor(SqlConnection cn)
        {
            _cn = cn;
        }

        public InstantSeriesSqlExecutor(string cnstring)
        {
            _cn = new SqlConnection(cnstring);
        }

        public bool ExecuteBulk(
            InstantSeriesItem[] items,
            string buforTable,
            SqlDbBulkPrepareType prepareType = SqlDbBulkPrepareType.None,
            SqlDbBulkType dbType = SqlDbBulkType.TempDB
        )
        {
            try
            {
                IInstantSeries catalog = null;
                if (items.Any())
                {
                    catalog = items.ElementAt(0).Figures;
                    if (_cn.State == ConnectionState.Closed)
                        _cn.Open();
                    try
                    {
                        if (dbType == SqlDbBulkType.TempDB)
                            _cn.ChangeDatabase("tempdb");
                        if (
                            !SqlDbRegistry.Temp.Tables.Have(buforTable)
                            || prepareType == SqlDbBulkPrepareType.Drop
                        )
                        {
                            string createTable = "";
                            if (prepareType == SqlDbBulkPrepareType.Drop)
                                createTable += "Drop table if exists [" + buforTable + "] \n";
                            createTable += "GetCreator Table [" + buforTable + "] ( ";
                            foreach (MemberRubric column in catalog.Rubrics.AsValues())
                            {
                                string sqlTypeString = "varchar(200)";
                                List<string> defineStr = new List<string>()
                                {
                                    "varchar",
                                    "nvarchar",
                                    "ntext",
                                    "varbinary"
                                };
                                List<string> defineDec = new List<string>()
                                {
                                    "decimal",
                                    "numeric"
                                };
                                int colLenght = column.RubricSize;
                                sqlTypeString = SqlDbNetType.NetTypeToSql(column.RubricType);
                                string addSize =
                                    (colLenght > 0)
                                        ? (defineStr.Contains(sqlTypeString))
                                            ? (string.Format(@"({0})", colLenght))
                                            : (defineDec.Contains(sqlTypeString))
                                                ? (string.Format(@"({0}, {1})", colLenght - 6, 6))
                                                : ""
                                        : "";
                                sqlTypeString += addSize;
                                createTable +=
                                    " [" + column.RubricName + "] " + sqlTypeString + ",";
                            }
                            createTable = createTable.TrimEnd(new char[] { ',' }) + " ) ";
                            SqlCommand createcmd = new SqlCommand(createTable, _cn);
                            createcmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new SqlDbCommandInsertException(ex.ToString());
                    }
                    if (prepareType == SqlDbBulkPrepareType.Trunc)
                    {
                        string deleteData = "Truncate Table [" + buforTable + "]";
                        SqlCommand delcmd = new SqlCommand(deleteData, _cn);
                        delcmd.ExecuteNonQuery();
                    }

                    try
                    {
                        InstantSeriesSqlReader ndr = new InstantSeriesSqlReader(items);
                        SqlBulkCopy bulkcopy = new SqlBulkCopy(_cn);
                        bulkcopy.DestinationTableName = "[" + buforTable + "]";
                        bulkcopy.WriteToServer(ndr);
                    }
                    catch (SqlException ex)
                    {
                        throw new SqlDbCommandInsertException(ex.ToString());
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (SqlException ex)
            {
                throw new SqlDbCommandInsertException(ex.ToString());
            }
        }

        public bool ExecuteBulk(
            IInstantSeries catalog,
            string buforTable,
            SqlDbBulkPrepareType prepareType = SqlDbBulkPrepareType.None,
            SqlDbBulkType dbType = SqlDbBulkType.TempDB
        )
        {
            try
            {
                if (_cn.State == ConnectionState.Closed)
                    _cn.Open();
                try
                {
                    if (dbType == SqlDbBulkType.TempDB)
                        _cn.ChangeDatabase("tempdb");
                    if (
                        !SqlDbRegistry.Schema.Tables.Have(buforTable)
                        || prepareType == SqlDbBulkPrepareType.Drop
                    )
                    {
                        string createTable = "";
                        if (prepareType == SqlDbBulkPrepareType.Drop)
                            createTable += "Drop table if exists [" + buforTable + "] \n";
                        createTable += "GetCreator Table [" + buforTable + "] ( ";
                        foreach (MemberRubric column in catalog.Rubrics.AsValues())
                        {
                            string sqlTypeString = "varchar(200)";
                            List<string> defineOne = new List<string>()
                            {
                                "varchar",
                                "nvarchar",
                                "ntext",
                                "varbinary"
                            };
                            List<string> defineDec = new List<string>() { "decimal", "numeric" };
                            int colLenght = column.RubricSize;
                            sqlTypeString = SqlDbNetType.NetTypeToSql(column.RubricType);
                            string addSize =
                                (colLenght > 0)
                                    ? (defineOne.Contains(sqlTypeString))
                                        ? (string.Format(@"({0})", colLenght))
                                        : (defineDec.Contains(sqlTypeString))
                                            ? (string.Format(@"({0}, {1})", colLenght - 6, 6))
                                            : ""
                                    : "";
                            sqlTypeString += addSize;
                            createTable += " [" + column.RubricName + "] " + sqlTypeString + ",";
                        }
                        createTable = createTable.TrimEnd(new char[] { ',' }) + " ) ";
                        SqlCommand createcmd = new SqlCommand(createTable, _cn);
                        createcmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    throw new SqlDbCommandInsertException(ex.ToString());
                }
                if (prepareType == SqlDbBulkPrepareType.Trunc)
                {
                    string deleteData = "Truncate Table [" + buforTable + "]";
                    SqlCommand delcmd = new SqlCommand(deleteData, _cn);
                    delcmd.ExecuteNonQuery();
                }

                try
                {
                    InstantSeriesSqlReader ndr = new InstantSeriesSqlReader(catalog);
                    SqlBulkCopy bulkcopy = new SqlBulkCopy(_cn);
                    bulkcopy.DestinationTableName = "[" + buforTable + "]";
                    bulkcopy.WriteToServer(ndr);
                }
                catch (SqlException ex)
                {
                    throw new SqlDbCommandInsertException(ex.ToString());
                }
                return true;
            }
            catch (SqlException ex)
            {
                throw new SqlDbCommandInsertException(ex.ToString());
            }
        }

        public int ExecuteDelete(string sqlqry, bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            SqlTransaction tr = _cn.BeginTransaction();
            cmd.Transaction = tr;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            int i = cmd.ExecuteNonQuery();
            tr.Commit();
            if (disposeCmd)
                cmd.Dispose();
            return i;
        }

        public ISeries<ISeries<IInstant>> ExecuteDelete(
            string sqlqry,
            IInstantSeries items,
            bool disposeCmd = false
        )
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            InstantSeriesSqlLoader<IInstantSeries> dr = new InstantSeriesSqlLoader<IInstantSeries>(sdr);
            var _is = dr.LoadDelete(items);
            sdr.Dispose();
            if (disposeCmd)
                cmd.Dispose();
            return _is;
        }

        public IInstantSeries ExecuteSelect(string sqlqry, string tableName = null)
        {
            SqlCommand cmd = new SqlCommand(sqlqry, _cn);
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            InstantSeriesSqlLoader<IInstantSeries> dr = new InstantSeriesSqlLoader<IInstantSeries>(sdr);
            IInstantSeries it = dr.LoadSelect(tableName);
            sdr.Dispose();
            cmd.Dispose();
            return it;
        }

        public IInstantSeries ExecuteSelect(
            string sqlqry,
            string tableName,
            ISeries<string> keyNames = null
        )
        {
            try
            {
                SqlCommand cmd = new SqlCommand(sqlqry, _cn);
                cmd.Prepare();
                if (_cn.State == ConnectionState.Closed)
                    _cn.Open();
                IDataReader sdr = cmd.ExecuteReader();
                InstantSeriesSqlLoader<IInstantSeries> dr = new InstantSeriesSqlLoader<IInstantSeries>(sdr);
                IInstantSeries it = dr.LoadSelect(tableName, keyNames);
                sdr.Dispose();
                cmd.Dispose();
                return it;
            }
            catch (Exception ex)
            {
                throw new SqlException(ex.ToString());
            }
        }

        public int ExecuteInsert(string sqlqry, bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            SqlTransaction tr = _cn.BeginTransaction();
            cmd.Transaction = tr;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            int i = cmd.ExecuteNonQuery();
            tr.Commit();
            if (disposeCmd)
                cmd.Dispose();
            return i;
        }

        public ISeries<ISeries<IInstant>> ExecuteInsert(
            string sqlqry,
            IInstantSeries items,
            bool disposeCmd = false
        )
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            InstantSeriesSqlLoader<IInstantSeries> dr = new InstantSeriesSqlLoader<IInstantSeries>(sdr);
            var _is = dr.LoadInsert(items);
            sdr.Dispose();
            if (disposeCmd)
                cmd.Dispose();
            return _is;
        }

        public int ExecuteUpdate(string sqlqry, bool disposeCmd = false)
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            SqlTransaction tr = _cn.BeginTransaction();
            cmd.Transaction = tr;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            int i = cmd.ExecuteNonQuery();
            tr.Commit();
            if (disposeCmd)
                cmd.Dispose();
            return i;
        }

        public ISeries<ISeries<IInstant>> ExecuteUpdate(
            string sqlqry,
            IInstantSeries items,
            bool disposeCmd = false
        )
        {
            if (_cmd == null)
                _cmd = _cn.CreateCommand();
            SqlCommand cmd = _cmd;
            cmd.CommandText = sqlqry;
            cmd.Prepare();
            if (_cn.State == ConnectionState.Closed)
                _cn.Open();
            IDataReader sdr = cmd.ExecuteReader();
            InstantSeriesSqlLoader<IInstantSeries> dr = new InstantSeriesSqlLoader<IInstantSeries>(sdr);
            var _is = dr.LoadUpdate(items);
            sdr.Dispose();
            if (disposeCmd)
                cmd.Dispose();
            return _is;
        }
    }
}
