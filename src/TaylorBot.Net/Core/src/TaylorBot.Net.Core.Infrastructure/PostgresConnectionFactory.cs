using Npgsql;

namespace TaylorBot.Net.Core.Infrastructure
{
    public class PostgresConnectionFactory
    {
        private readonly NpgsqlDataSource _npgsqlDataSource;

        public PostgresConnectionFactory(NpgsqlDataSource npgsqlDataSource)
        {
            _npgsqlDataSource = npgsqlDataSource;
        }

        public NpgsqlConnection CreateConnection() => _npgsqlDataSource.CreateConnection();
    }
}
