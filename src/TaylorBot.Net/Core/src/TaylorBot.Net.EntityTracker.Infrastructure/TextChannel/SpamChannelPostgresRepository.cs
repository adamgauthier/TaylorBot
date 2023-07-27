using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.EntityTracker.Infrastructure.TextChannel;

public class SpamChannelPostgresRepository : ISpamChannelRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public SpamChannelPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask<bool> InsertOrGetIsSpamChannelAsync(ITextChannel channel)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

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

    private async ValueTask UpsertSpamChannelAsync(ITextChannel channel, bool isSpam)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

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

    public async ValueTask AddSpamChannelAsync(ITextChannel channel)
    {
        await UpsertSpamChannelAsync(channel, isSpam: true);
    }

    public async ValueTask RemoveSpamChannelAsync(ITextChannel channel)
    {
        await UpsertSpamChannelAsync(channel, isSpam: false);
    }
}
