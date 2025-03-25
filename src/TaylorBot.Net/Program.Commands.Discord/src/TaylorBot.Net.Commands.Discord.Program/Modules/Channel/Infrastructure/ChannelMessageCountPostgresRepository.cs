using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Commands;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Infrastructure;

public class ChannelMessageCountPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IChannelMessageCountRepository
{
    private sealed record CountDto(long message_count, bool is_spam);

    public async Task<MessageCount> GetMessageCountAsync(GuildTextChannel channel)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var count = await connection.QuerySingleAsync<CountDto>(
            """
            SELECT message_count, is_spam FROM guilds.text_channels
            WHERE guild_id = @GuildId AND channel_id = @ChannelId;
            """,
            new
            {
                GuildId = $"{channel.GuildId}",
                ChannelId = $"{channel.Id}",
            }
        );

        return new(count.message_count, count.is_spam);
    }
}
