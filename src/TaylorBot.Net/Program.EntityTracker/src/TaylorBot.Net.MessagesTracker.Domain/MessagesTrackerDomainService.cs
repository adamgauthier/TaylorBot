using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.MinutesTracker.Domain.Options;

namespace TaylorBot.Net.MessagesTracker.Domain
{
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

    public class MessagesTrackerDomainService
    {
        private readonly ILogger<MessagesTrackerDomainService> _logger;
        private readonly IOptionsMonitor<MessagesTrackerOptions> _messagesTrackerOptions;
        private readonly ISpamChannelRepository _spamChannelRepository;
        private readonly ITextChannelMessageCountRepository _textChannelMessageCountRepository;
        private readonly IGuildUserLastSpokeRepository _guildUserLastSpokeRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly WordCounter _wordCounter;

        public MessagesTrackerDomainService(
            ILogger<MessagesTrackerDomainService> logger,
            IOptionsMonitor<MessagesTrackerOptions> messagesTrackerOptions,
            ISpamChannelRepository spamChannelRepository,
            ITextChannelMessageCountRepository textChannelMessageCountRepository,
            IGuildUserLastSpokeRepository guildUserLastSpokeRepository,
            IMessageRepository messageRepository,
            WordCounter wordCounter
        )
        {
            _logger = logger;
            _messagesTrackerOptions = messagesTrackerOptions;
            _spamChannelRepository = spamChannelRepository;
            _textChannelMessageCountRepository = textChannelMessageCountRepository;
            _guildUserLastSpokeRepository = guildUserLastSpokeRepository;
            _messageRepository = messageRepository;
            _wordCounter = wordCounter;
        }

        public async ValueTask OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketGuildUser guildUser, SocketUserMessage message)
        {
            var isSpam = await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);

            if (!isSpam)
            {
                await _messageRepository.QueueAddMessagesAndWordsAsync(guildUser, 1, _wordCounter.CountWords(message.Content));
            }

            await _guildUserLastSpokeRepository.QueueUpdateLastSpokeAsync(guildUser, message.Timestamp);
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
                    _logger.LogError(e, $"Unhandled exception in {nameof(_textChannelMessageCountRepository.PersistQueuedMessageCountIncrementsAsync)}.");
                }

                await Task.Delay(_messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingTextChannelMessages);
            }
        }

        public async Task StartPersistingMemberMessagesAndWordsAsync()
        {
            while (true)
            {
                try
                {
                    await _messageRepository.PersistQueuedMessagesAndWordsAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(_messageRepository.PersistQueuedMessagesAndWordsAsync)}.");
                }

                await Task.Delay(_messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingMemberMessagesAndWords);
            }
        }

        public async Task StartPersistingLastSpokeAsync()
        {
            while (true)
            {
                try
                {
                    await _guildUserLastSpokeRepository.PersistQueuedLastSpokeUpdatesAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(_guildUserLastSpokeRepository.PersistQueuedLastSpokeUpdatesAsync)}.");
                }

                await Task.Delay(_messagesTrackerOptions.CurrentValue.TimeSpanBetweenPersistingLastSpoke);
            }
        }
    }
}
