namespace Radical.Instant.Series.Sql
{
    using Microsoft.Data.SqlClient;
    using Radical.Series;
    using Db;
    using Db.Command;

    public class InstantSeriesSqlDb
    {
        private InstantSeriesSqlAccessor accessor;
        private SqlDbCommandDelete delete;
        private InstantSeriesSqlIdentity identity;
        private SqlDbCommandInsert insert;
        private InstantSeriesSqlMapper mapper;
        private InstantSeriesSqlMutator mutator;
        private SqlConnection sqlcn;
        private SqlDbCommandUpdate update;

        public InstantSeriesSqlDb(SqlConnection SqlDbConnection) : this(SqlDbConnection.ConnectionString) { }

        public InstantSeriesSqlDb(InstantSeriesSqlIdentity sqlIdentity)
        {
            identity = sqlIdentity;
            sqlcn = new SqlConnection(cnString);
            Initialization();
        }

        public InstantSeriesSqlDb(string SqlConnectionString)
        {
            identity = new InstantSeriesSqlIdentity();
            cnString = SqlConnectionString;
            sqlcn = new SqlConnection(cnString);
            Initialization();
        }

        private string cnString
        {
            get => identity.ConnectionString;
            set => identity.ConnectionString = value;
        }

        public IInstantSeries Get(string sqlQry, string tableName, ISeries<string> keyNames = null)
        {
            return accessor.Get(cnString, sqlQry, tableName, keyNames);
        }

        public ISeries<ISeries<IInstant>> Add(IInstantSeries items)
        {
            return mutator.Set(cnString, items, false);
        }

        public ISeries<ISeries<IInstant>> BatchDelete(IInstantSeries items, bool buildMapping)
        {
            if (delete == null)
                delete = new SqlDbCommandDelete(sqlcn);
            return delete.BatchDelete(items, buildMapping);
        }

        public ISeries<ISeries<IInstant>> BatchInsert(IInstantSeries items, bool buildMapping)
        {
            if (insert == null)
                insert = new SqlDbCommandInsert(sqlcn);
            return insert.BatchInsert(items, buildMapping);
        }

        public ISeries<ISeries<IInstant>> BatchUpdate(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            bool updateKeys = false,
            string[] updateExcept = null
        )
        {
            if (update == null)
                update = new SqlDbCommandUpdate(sqlcn);
            return update.BatchUpdate(items, keysFromDeck, buildMapping, updateKeys, updateExcept);
        }

        public ISeries<ISeries<IInstant>> BulkDelete(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            if (delete == null)
                delete = new SqlDbCommandDelete(sqlcn);
            return delete.Delete(items, keysFromDeck, buildMapping, tempType);
        }

        public ISeries<ISeries<IInstant>> BulkInsert(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            if (insert == null)
                insert = new SqlDbCommandInsert(sqlcn);
            return insert.Insert(items, keysFromDeck, buildMapping, false, null, tempType);
        }

        public ISeries<ISeries<IInstant>> BulkUpdate(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            bool updateKeys = false,
            string[] updateExcept = null,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            if (update == null)
                update = new SqlDbCommandUpdate(sqlcn);
            return update.BulkUpdate(
                items,
                keysFromDeck,
                buildMapping,
                updateKeys,
                updateExcept,
                tempType
            );
        }

        public ISeries<ISeries<IInstant>> Delete(IInstantSeries items)
        {
            return mutator.Delete(cnString, items);
        }

        public ISeries<ISeries<IInstant>> Delete(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            return BulkDelete(items, keysFromDeck, buildMapping, tempType);
        }

        public int Execute(string query)
        {
            SqlCommand cmd = sqlcn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = query;
            return cmd.ExecuteNonQuery();
        }

        public ISeries<ISeries<IInstant>> Insert(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            return BulkInsert(items, keysFromDeck, buildMapping, tempType);
        }

        public IInstantSeries Mapper(
            IInstantSeries items,
            bool keysFromDeck = false,
            string[] dbTableNames = null,
            string tablePrefix = ""
        )
        {
            mapper = new InstantSeriesSqlMapper(items, keysFromDeck, dbTableNames, tablePrefix);
            return mapper.ItemsMapped;
        }

        public ISeries<ISeries<IInstant>> Put(IInstantSeries items)
        {
            return mutator.Set(cnString, items, true);
        }

        public int SimpleDelete(IInstantSeries items)
        {
            if (delete == null)
                delete = new SqlDbCommandDelete(sqlcn);
            return delete.SimpleDelete(items);
        }

        public int SimpleDelete(IInstantSeries items, bool buildMapping)
        {
            if (delete == null)
                delete = new SqlDbCommandDelete(sqlcn);
            return delete.SimpleDelete(items, buildMapping);
        }

        public int SimpleInsert(IInstantSeries items)
        {
            if (insert == null)
                insert = new SqlDbCommandInsert(sqlcn);
            return insert.SimpleInsert(items);
        }

        public int SimpleInsert(IInstantSeries items, bool buildMapping)
        {
            if (insert == null)
                insert = new SqlDbCommandInsert(sqlcn);
            return insert.SimpleInsert(items, buildMapping);
        }

        public int SimpleUpdate(
            IInstantSeries items,
            bool buildMapping = false,
            bool updateKeys = false,
            string[] updateExcept = null
        )
        {
            if (update == null)
                update = new SqlDbCommandUpdate(sqlcn);
            return update.SimpleUpdate(items, buildMapping, updateKeys, updateExcept);
        }

        public ISeries<ISeries<IInstant>> Update(
            IInstantSeries items,
            bool keysFromDeck = false,
            bool buildMapping = false,
            bool updateKeys = false,
            string[] updateExcept = null,
            SqlDbBulkPrepareType tempType = SqlDbBulkPrepareType.Trunc
        )
        {
            return BulkUpdate(
                items,
                keysFromDeck,
                buildMapping,
                updateKeys,
                updateExcept,
                tempType
            );
        }

        private void Initialization()
        {
            string dbName = sqlcn.Database;
            SqlDbSchemaBuild SchemaBuild = new SqlDbSchemaBuild(sqlcn);
            SchemaBuild.SchemaPrepare();
            sqlcn.ChangeDatabase("tempdb");
            SchemaBuild.SchemaPrepare(BuildDbSchemaType.Temp);
            sqlcn.ChangeDatabase(dbName);
            accessor = new InstantSeriesSqlAccessor();
            mutator = new InstantSeriesSqlMutator(this);
        }
    }

    public class SqlException : Exception
    {
        public SqlException(string message) : base(message) { }
    }
}
