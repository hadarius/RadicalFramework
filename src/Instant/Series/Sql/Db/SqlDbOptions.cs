namespace Radical.Instant.Series.Sql.Db
{
    public class SqlDbOptions
    {
        public SqlDbOptions() { }

        public SqlDbOptions(
            string _User,
            string _Password,
            string _Source,
            string _Catalog,
            string _Provider = "SQLNCLI11"
        )
        {
            User = _User;
            Password = _Password;
            Provider = _Provider;
            Source = _Source;
            InitCatalog = _Catalog;
            DbConnectionString = string.Format(
                "Provider={0};Data Source = {1}; Persist Security Info=True;Password={2};User ID = {3}; Initial Registry = {4}",
                Provider,
                Source,
                Password,
                User,
                InitCatalog
            );
        }

        public SqlDbOptions(string dbConnectionString)
        {
            DbConnectionString = dbConnectionString;
        }

        public string InitCatalog { get; set; }

        public string DbConnectionString { get; set; }

        public string Password { get; set; }

        public string Provider { get; set; }

        public string Source { get; set; }

        public string User { get; set; }
    }
}
