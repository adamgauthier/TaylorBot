using Dapper;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;

public class SpamChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ISpamChannelRepository
{
    public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(GuildTextChannel channel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            """
            INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES (@GuildId, @ChannelId)
            ON CONFLICT (guild_id, channel_id) DO UPDATE SET
                registered_at = guilds.text_channels.registered_at
            RETURNING is_spam;
            """,
            new
            {
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
            }
        );
    }

    private async ValueTask UpsertSpamChannelAsync(GuildTextChannel channel, bool isSpam)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO guilds.text_channels (guild_id, channel_id, is_spam) VALUES (@GuildId, @ChannelId, @IsSpam)
            ON CONFLICT (guild_id, channel_id) DO UPDATE SET
                is_spam = excluded.is_spam;
            """,
            new
            {
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
                IsSpam = isSpam,
            }
        );
    }

    public async ValueTask AddSpamChannelAsync(GuildTextChannel channel)
    {
        await UpsertSpamChannelAsync(channel, isSpam: true);
    }

    public async ValueTask RemoveSpamChannelAsync(GuildTextChannel channel)
    {
        await UpsertSpamChannelAsync(channel, isSpam: false);
    }
}
