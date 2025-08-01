﻿using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Infrastructure;

public class ModLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IModLogChannelRepository
{
    public async ValueTask AddOrUpdateModLogAsync(GuildTextChannel textChannel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO moderation.mod_log_channels (guild_id, channel_id)
            VALUES (@GuildId, @ChannelId)
            ON CONFLICT (guild_id) DO UPDATE SET
                channel_id = excluded.channel_id;
            """,
            new
            {
                GuildId = $"{textChannel.GuildId}",
                ChannelId = $"{textChannel.Id}",
            }
        );
    }

    public async ValueTask RemoveModLogAsync(CommandGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM moderation.mod_log_channels WHERE guild_id = @GuildId;",
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }

    private sealed record LogChannelDto(string channel_id);

    public async ValueTask<ModLog?> GetModLogForGuildAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var logChannel = await connection.QuerySingleOrDefaultAsync<LogChannelDto?>(
            """
            SELECT channel_id FROM moderation.mod_log_channels
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );

        return logChannel != null ? new ModLog(new SnowflakeId(logChannel.channel_id)) : null;
    }
}
