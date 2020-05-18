using Dapper;
using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MessagesTracker.Infrastructure.Models;

namespace TaylorBot.Net.MessagesTracker.Infrastructure
{
    public class MessagesRepository : IMessageRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MessagesRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async Task<ChannelMessageCountChanged> AddChannelMessageCountAsync(ITextChannel textChannel, long messageCountToAdd)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            var textChannelMessageCountAddedDto = await connection.QuerySingleAsync<ChannelMessageCountChangedDto>(
                @"UPDATE guilds.text_channels
                    SET message_count = message_count + @MessageCountToAdd
                    WHERE guild_id = @GuildId AND channel_id = @ChannelId
                    RETURNING is_spam;",
                new
                {
                    MessageCountToAdd = messageCountToAdd,
                    GuildId = textChannel.GuildId.ToString(),
                    ChannelId = textChannel.Id.ToString()
                }
            );

            return new ChannelMessageCountChanged(textChannelMessageCountAddedDto.is_spam);
        }

        public async Task AddMessagesWordsAndLastSpokeAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd, DateTime lastSpokeAt)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE guilds.guild_members SET
                       message_count = message_count + @MessageCountToAdd,
                       word_count = word_count + @WordCountToAdd,
                       last_spoke_at = @LastSpokeAt
                    WHERE guild_id = @GuildId AND user_id = @UserId;",
                new
                {
                    MessageCountToAdd = messageCountToAdd,
                    WordCountToAdd = wordCountToAdd,
                    LastSpokeAt = lastSpokeAt,
                    GuildId = guildUser.GuildId.ToString(),
                    UserId = guildUser.Id.ToString()
                }
            );
        }

        public async Task UpdateLastSpokeAsync(IGuildUser guildUser, DateTime lastSpokeAt)
        {
            using var connection = _postgresConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                @"UPDATE guilds.guild_members SET last_spoke_at = @LastSpokeAt
                    WHERE guild_id = @GuildId AND user_id = @UserId;",
                new
                {
                    LastSpokeAt = lastSpokeAt,
                    GuildId = guildUser.GuildId.ToString(),
                    UserId = guildUser.Id.ToString()
                }
            );
        }
    }
}
