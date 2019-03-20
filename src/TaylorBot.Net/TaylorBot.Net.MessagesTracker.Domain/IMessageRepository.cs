using Discord;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.MessagesTracker.Domain
{
    public interface IMessageRepository
    {
        Task<ChannelMessageCountChanged> AddChannelMessageCountAsync(ITextChannel textChannel, long messageCountToAdd);
        Task AddMessagesWordsAndLastSpokeAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd, DateTime lastSpokeAt);
        Task UpdateLastSpokeAsync(IGuildUser guildUser, DateTime lastSpokeAt);
    }
}
