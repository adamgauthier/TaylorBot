using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Core.Infrastructure
{
    public abstract class PostgresRepository
    {
        private IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor;

        protected PostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        protected IDbConnection Connection
        {
            get
            {
                var options = optionsMonitor.CurrentValue;
                return new NpgsqlConnection(
                    $"Server={options.Host};Port={options.Port};Username={options.Username};Password={options.Password};Database={options.Database}"
                );
            }
        }
    }
}
