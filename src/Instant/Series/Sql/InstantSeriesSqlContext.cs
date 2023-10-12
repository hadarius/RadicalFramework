using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Radical.Instant.Series.Sql
{
    public class InstantSeriesSqlContext : InstantSeriesSqlDb
    {
        public InstantSeriesSqlContext(InstantSeriesSqlIdentity identity) : base(identity) { }

        public InstantSeriesSqlContext(string connectionString) : base(connectionString) { }

        public InstantSeriesSqlContext(IConfiguration configuration, string connectionName)
            : base(configuration.GetConnectionString(connectionName)) { }

        public InstantSeriesSqlContext(IConfiguration configuration)
            : base(
                configuration.GetSection("ConnectionString")?.GetChildren()?.FirstOrDefault()?.Value
            )
        { }
    }
}
