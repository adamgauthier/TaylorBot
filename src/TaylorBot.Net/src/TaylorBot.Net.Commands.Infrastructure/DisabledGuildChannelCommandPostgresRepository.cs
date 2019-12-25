using Dapper;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;

namespace TaylorBot.Net.Commands.Infrastructure
{
    public class DisabledGuildChannelCommandPostgresRepository : PostgresRepository, IDisabledGuildChannelCommandRepository
    {
        public DisabledGuildChannelCommandPostgresRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<bool> IsGuildChannelCommandDisabledAsync(ITextChannel textChannel, CommandInfo command)
        {
            using var connection = Connection;
            connection.Open();

            var disabled = await connection.QuerySingleOrDefaultAsync<bool>(
                @"SELECT EXISTS(
                    SELECT 1 FROM guilds.channel_commands
                    WHERE guild_id = @GuildId AND channel_id = @ChannelId AND command_id = @CommandId
                );",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString(),
                    CommandId = command.Name
                }
            );

            return disabled;
        }
    }
}
