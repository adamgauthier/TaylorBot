using Npgsql;

namespace TaylorBot.Net.Core.Infrastructure;

public class PostgresConnectionFactory(NpgsqlDataSource npgsqlDataSource)
{
    public NpgsqlConnection CreateConnection() => npgsqlDataSource.CreateConnection();
}
