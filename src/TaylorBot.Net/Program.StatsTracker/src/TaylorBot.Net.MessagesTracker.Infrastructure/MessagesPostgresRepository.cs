using Dapper;
using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.MessagesTracker.Infrastructure
{
    public class MessagesPostgresRepository : IMessageRepository
    {
        private readonly PostgresConnectionFactory _postgresConnectionFactory;

        public MessagesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory)
        {
            _postgresConnectionFactory = postgresConnectionFactory;
        }

        public async ValueTask AddMessagesWordsAndLastSpokeAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd, DateTime lastSpokeAt)
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

        public async ValueTask UpdateLastSpokeAsync(IGuildUser guildUser, DateTime lastSpokeAt)
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
