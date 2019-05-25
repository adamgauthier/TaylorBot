using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using TaylorBot.Net.EntityTracker.Domain.GuildName;

namespace TaylorBot.Net.EntityTracker.Infrastructure.GuildName
{
    public class GuildNameRepository : PostgresRepository, IGuildNameRepository
    {
        public GuildNameRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task AddNewGuildNameAsync(IGuild guild)
        {
            using (var connection = Connection)
            {
                connection.Open();

                await connection.ExecuteAsync(
                    "INSERT INTO guilds.guild_names (guild_id, guild_name) VALUES (@GuildId, @GuildName);",
                    new
                    {
                        GuildId = guild.Id.ToString(),
                        GuildName = guild.Name
                    }
                );
            }
        }

        public async Task<string> GetLatestGuildNameAsync(IGuild guild)
        {
            using (var connection = Connection)
            {
                connection.Open();

                var latestGuildNameDto = await connection.QuerySingleOrDefaultAsync<LatestGuildNameDto>(
                    @"SELECT g.guild_name, g.guild_id
                    FROM (
                        SELECT guild_id, MAX(changed_at) AS max_changed_at
                        FROM guilds.guild_names
                        GROUP BY guild_id
                    ) AS maxed
                    JOIN guilds.guild_names AS g ON g.guild_id = maxed.guild_id AND g.changed_at = maxed.max_changed_at
                    WHERE g.guild_id = @GuildId;",
                    new
                    {
                        GuildId = guild.Id.ToString()
                    }
                );

                return latestGuildNameDto?.guild_name;
            }
        }
    }
}
