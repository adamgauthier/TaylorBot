using Dapper;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Infrastructure
{
    public class BotInfoRepositoryPostgresRepository : IBotInfoRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public BotInfoRepositoryPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        private class ProductVersionDto
        {
            public string info_value { get; set; } = null!;
        }

        public async ValueTask<string> GetProductVersionAsync()
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var productVersionDto = await connection.QuerySingleAsync<ProductVersionDto>(
                "SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version';"
            );

            return productVersionDto.info_value;
        }
    }
}
