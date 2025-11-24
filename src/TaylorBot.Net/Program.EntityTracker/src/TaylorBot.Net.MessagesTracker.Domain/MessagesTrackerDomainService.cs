using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.MessagesTracker.Domain.Options;

namespace TaylorBot.Net.MessagesTracker.Domain;

public interface ITextChannelMessageCountRepository
{
    ValueTask QueueIncrementMessageCountAsync(ITextChannel channel);
    ValueTask PersistQueuedMessageCountIncrementsAsync();
}

public interface IGuildUserLastSpokeRepository
{
    ValueTask QueueUpdateLastSpokeAsync(IGuildUser guildUser, DateTimeOffset lastSpokeAt);
    ValueTask PersistQueuedLastSpokeUpdatesAsync();
}

public interface IMessageRepository
{
    ValueTask QueueAddMessagesAndWordsAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd);
    ValueTask PersistQueuedMessagesAndWordsAsync();
}

public partial class MessagesTrackerDomainService(
    ILogger<MessagesTrackerDomainService> logger,
    IOptionsMonitor<MessagesTrackerOptions> messagesTrackerOptions,
    ISpamChannelRepository spamChannelRepository,
    ITextChannelMessageCountRepository textChannelMessageCountRepository,
    IGuildUserLastSpokeRepository guildUserLastSpokeRepository,
    IMessageRepository messageRepository,
    WordCounter wordCounter
    )
{
    public async ValueTask OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketGuildUser guildUser, SocketUserMessage message)
    {
        var isSpam = await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(textChannel.Id, textChannel.Guild.Id, textChannel.GetChannelType() ?? ChannelType.Text));

        if (!isSpam)
        {
            await messageRepository.QueueAddMessagesAndWordsAsync(guildUser, 1, wordCounter.CountWords(message.Content));
        }

        await guildUserLastSpokeRepository.QueueUpdateLastSpokeAsync(guildUser, message.Timestamp);
        await textChannelMessageCountRepository.QueueIncrementMessageCountAsync(textChannel);
    }

    public async Task StartPersistingTextChannelMessageCountAsync()
    {
        while (true)
        {
            try
            {
                await textChannelMessageCountRepository.PersistQueuedMessageCountIncrementsAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionPersistingTextChannelMessages(e);
            }

            await Task.Delay(messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingTextChannelMessages);
        }
    }

    public async Task StartPersistingMemberMessagesAndWordsAsync()
    {
        while (true)
        {
            try
            {
                await messageRepository.PersistQueuedMessagesAndWordsAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionPersistingMemberMessages(e);
            }

            await Task.Delay(messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingMemberMessagesAndWords);
        }
    }

    public async Task StartPersistingLastSpokeAsync()
    {
        while (true)
        {
            try
            {
                await guildUserLastSpokeRepository.PersistQueuedLastSpokeUpdatesAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionPersistingLastSpoke(e);
            }

            await Task.Delay(messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingLastSpoke);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in PersistQueuedMessageCountIncrementsAsync.")]
    private partial void LogUnhandledExceptionPersistingTextChannelMessages(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in PersistQueuedMessagesAndWordsAsync.")]
    private partial void LogUnhandledExceptionPersistingMemberMessages(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in PersistQueuedLastSpokeUpdatesAsync.")]
    private partial void LogUnhandledExceptionPersistingLastSpoke(Exception exception);
}
