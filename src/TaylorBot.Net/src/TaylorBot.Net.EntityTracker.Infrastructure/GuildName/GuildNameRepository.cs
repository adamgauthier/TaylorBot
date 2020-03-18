using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
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
            using var connection = Connection;

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
}
