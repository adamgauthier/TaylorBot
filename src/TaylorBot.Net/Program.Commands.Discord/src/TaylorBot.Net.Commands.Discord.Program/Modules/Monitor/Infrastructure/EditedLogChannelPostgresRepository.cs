using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Infrastructure
{
    public class EditedLogChannelPostgresRepository : IEditedLogChannelRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public EditedLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddOrUpdateEditedLogAsync(ITextChannel textChannel)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"INSERT INTO plus.edited_log_channels (guild_id, edited_log_channel_id)
                VALUES (@GuildId, @ChannelId)
                ON CONFLICT (guild_id) DO UPDATE SET
                    edited_log_channel_id = excluded.edited_log_channel_id;",
                new
                {
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );
        }

        private class LogChannelDto
        {
            public string edited_log_channel_id { get; set; } = null!;
        }

        public async ValueTask<EditedLog?> GetEditedLogForGuildAsync(IGuild guild)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
                @"SELECT edited_log_channel_id FROM plus.edited_log_channels
                WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );

            return logChannel != null ? new EditedLog(new SnowflakeId(logChannel.edited_log_channel_id)) : null;
        }

        public async ValueTask RemoveEditedLogAsync(IGuild guild)
        {
            await using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "DELETE FROM plus.edited_log_channels WHERE guild_id = @GuildId;",
                new
                {
                    GuildId = guild.Id.ToString()
                }
            );
        }
    }
}
