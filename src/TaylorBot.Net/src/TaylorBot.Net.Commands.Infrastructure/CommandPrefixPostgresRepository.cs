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
                @"INSERT INTO guilds.guilds (guild_id) VALUES (@GuildId)
                  ON CONFLICT(guild_id) DO UPDATE SET prefix = guilds.guilds.prefix
                  RETURNING prefix;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );
        }
    }
}
