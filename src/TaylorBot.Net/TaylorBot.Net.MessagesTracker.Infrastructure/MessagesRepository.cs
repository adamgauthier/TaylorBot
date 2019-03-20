using Dapper;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.Core.Infrastructure;
using Discord;
using System;
using TaylorBot.Net.MessagesTracker.Infrastructure.Models;

namespace TaylorBot.Net.MessagesTracker.Infrastructure
{
    public class MessagesRepository : PostgresRepository, IMessageRepository
    {
        public MessagesRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<ChannelMessageCountChanged> AddChannelMessageCountAsync(ITextChannel textChannel, long messageCountToAdd)
        {
            using (var connection = Connection)
            {
                connection.Open();

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
        }

        public async Task AddMessagesWordsAndLastSpokeAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd, DateTime lastSpokeAt)
        {
            using (var connection = Connection)
            {
                connection.Open();

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
        }

        public async Task UpdateLastSpokeAsync(IGuildUser guildUser, DateTime lastSpokeAt)
        {
            using (var connection = Connection)
            {
                connection.Open();

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
}
