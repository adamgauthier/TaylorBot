using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Infrastructure;

public class ModMailLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IModMailLogChannelRepository
{
    public async ValueTask AddOrUpdateModMailLogAsync(ITextChannel textChannel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            @"INSERT INTO moderation.mod_mail_log_channels (guild_id, channel_id)
            VALUES (@GuildId, @ChannelId)
            ON CONFLICT (guild_id) DO UPDATE SET
                channel_id = excluded.channel_id;",
            new
            {
                GuildId = textChannel.GuildId.ToString(),
                ChannelId = textChannel.Id.ToString(),
            }
        );
    }

    public async ValueTask RemoveModMailLogAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM moderation.mod_mail_log_channels WHERE guild_id = @GuildId;",
            new
            {
                GuildId = guild.Id.ToString(),
            }
        );
    }

    private class LogChannelDto
    {
        public string channel_id { get; set; } = null!;
    }

    public async ValueTask<ModLog?> GetModMailLogForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
            @"SELECT channel_id FROM moderation.mod_mail_log_channels
            WHERE guild_id = @GuildId;",
            new
            {
                GuildId = guild.Id.ToString(),
            }
        );

        return logChannel != null ? new ModLog(new SnowflakeId(logChannel.channel_id)) : null;
    }
}
