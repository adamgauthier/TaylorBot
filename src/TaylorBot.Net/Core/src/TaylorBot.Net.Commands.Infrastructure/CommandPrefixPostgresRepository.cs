using Dapper;
using Discord;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class CommandPrefixPostgresRepository : PostgresRepository, ICommandPrefixRepository
    {
        public CommandPrefixPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<string> GetOrInsertGuildPrefixAsync(IGuild guild)
        {
            using var connection = Connection;

            return await connection.QuerySingleAsync<string>(
                @"INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES (@GuildId, @GuildName, NULL)
                ON CONFLICT (guild_id) DO UPDATE SET
                    previous_guild_name = guilds.guilds.guild_name,
                    guild_name = excluded.guild_name
                RETURNING prefix;",
                new
                {
                    GuildId = guild.Id.ToString(),
                    GuildName = guild.Name
                }
            );
        }
    }
}
