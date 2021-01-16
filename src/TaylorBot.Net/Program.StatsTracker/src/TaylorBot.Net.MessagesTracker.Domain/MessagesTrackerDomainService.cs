using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MessagesTracker.Domain
{
    public interface ITextChannelMessageCountRepository
    {
        ValueTask QueueIncrementMessageCountAsync(ITextChannel channel);
        ValueTask PersistQueuedMessageCountIncrementsAsync();
    }

    public interface IMessageRepository
    {
        ValueTask AddMessagesWordsAndLastSpokeAsync(IGuildUser guildUser, long messageCountToAdd, long wordCountToAdd, DateTime lastSpokeAt);
        ValueTask UpdateLastSpokeAsync(IGuildUser guildUser, DateTime lastSpokeAt);
    }

    public class MessagesTrackerDomainService
    {
        private readonly ILogger<MessagesTrackerDomainService> _logger;
        private readonly IOptionsMonitor<MessagesTrackerOptions> _messagesTrackerOptions;
        private readonly ISpamChannelRepository _spamChannelRepository;
        private readonly ITextChannelMessageCountRepository _textChannelMessageCountRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly WordCounter _wordCounter;

        public MessagesTrackerDomainService(
            ILogger<MessagesTrackerDomainService> logger,
            IOptionsMonitor<MessagesTrackerOptions> messagesTrackerOptions,
            ISpamChannelRepository spamChannelRepository,
            ITextChannelMessageCountRepository textChannelMessageCountRepository,
            IMessageRepository messageRepository,
            WordCounter wordCounter
        )
        {
            _logger = logger;
            _messagesTrackerOptions = messagesTrackerOptions;
            _spamChannelRepository = spamChannelRepository;
            _textChannelMessageCountRepository = textChannelMessageCountRepository;
            _messageRepository = messageRepository;
            _wordCounter = wordCounter;
        }

        public async ValueTask OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketGuildUser guildUser, SocketUserMessage message)
        {
            var isSpam = await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);

            if (!isSpam)
            {
                await _messageRepository.AddMessagesWordsAndLastSpokeAsync(guildUser, 1, _wordCounter.CountWords(message.Content), message.Timestamp.DateTime);
            }
            else
            {
                await _messageRepository.UpdateLastSpokeAsync(guildUser, message.Timestamp.DateTime);
            }

            await _textChannelMessageCountRepository.QueueIncrementMessageCountAsync(textChannel);
        }

        public async Task StartPersistingTextChannelMessageCountAsync()
        {
            while (true)
            {
                try
                {
                    await _textChannelMessageCountRepository.PersistQueuedMessageCountIncrementsAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, LogString.From($"Unhandled exception in {nameof(_textChannelMessageCountRepository.PersistQueuedMessageCountIncrementsAsync)}."));
                }

                await Task.Delay(_messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingTextChannelMessages);
            }
        }
    }
}
