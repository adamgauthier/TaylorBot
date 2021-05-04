using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Core.Infrastructure
{
    public class PostgresConnectionFactory
    {
        private readonly IOptionsMonitor<DatabaseConnectionOptions> _optionsMonitor;

        public PostgresConnectionFactory(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public IDbConnection CreateConnection()
        {
            var options = _optionsMonitor.CurrentValue;
            return new NpgsqlConnection(string.Join(';', new[] {
                $"Server={options.Host}",
                $"Port={options.Port}",
                $"Username={options.Username}",
                $"Password={options.Password}",
                $"Database={options.Database}",
                $"ApplicationName={options.ApplicationName}",
                "SSL Mode=Prefer",
                "Trust Server Certificate=true",
            }));
        }
    }
}
