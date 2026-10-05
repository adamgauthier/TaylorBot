using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Infrastructure;

public class ModLogChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory, IModLogChannelLookup modLogChannelLookup) : IModLogChannelRepository
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

    public async ValueTask<ModLog?> GetModLogForGuildAsync(IGuild guild)
    {
        var channelId = await modLogChannelLookup.GetChannelIdAsync(guild.Id);

        return channelId == null ? null : new ModLog(channelId);
    }
}
