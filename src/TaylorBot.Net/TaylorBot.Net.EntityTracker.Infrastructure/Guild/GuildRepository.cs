using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using System.Linq;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Guild
{
    public class GuildRepository : PostgresRepository, IGuildRepository
    {
        public GuildRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var results = await connection.QueryAsync(
                    @"INSERT INTO guilds.guilds (guild_id) VALUES (@GuildId)
                    ON CONFLICT (guild_id) DO NOTHING RETURNING guild_id;",
                    new
                    {
                        GuildId = guild.Id.ToString()
                    }
                );

                return new GuildAddedResult(wasAdded: results.Any());
            }
        }
    }
}
