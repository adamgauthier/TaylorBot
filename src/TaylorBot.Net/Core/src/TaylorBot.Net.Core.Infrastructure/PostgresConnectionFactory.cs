using Npgsql;
using System.Data;

namespace TaylorBot.Net.Core.Infrastructure
{
    public class PostgresConnectionFactory
    {
        private readonly NpgsqlDataSource _npgsqlDataSource;

        public PostgresConnectionFactory(NpgsqlDataSource npgsqlDataSource)
        {
            _npgsqlDataSource = npgsqlDataSource;
        }

        public IDbConnection CreateConnection() => _npgsqlDataSource.CreateConnection();
    }
}
