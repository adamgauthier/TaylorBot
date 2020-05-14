using Discord.WebSocket;
using System.Threading.Tasks;

namespace TaylorBot.Net.MessagesTracker.Domain
{
    public class MessagesTrackerDomainService
    {
        private readonly IMessageRepository messageRepository;
        private readonly WordCounter wordCounter;

        public MessagesTrackerDomainService(IMessageRepository messageRepository, WordCounter wordCounter)
        {
            this.messageRepository = messageRepository;
            this.wordCounter = wordCounter;
        }

        public async Task OnGuildUserMessageReceivedAsync(SocketTextChannel textChannel, SocketGuildUser guildUser, SocketUserMessage message)
        {
            var textChannelMessageCountAdded = await messageRepository.AddChannelMessageCountAsync(textChannel, 1);

            if (!textChannelMessageCountAdded.IsSpam)
            {
                await messageRepository.AddMessagesWordsAndLastSpokeAsync(guildUser, 1, wordCounter.CountWords(message.Content), message.Timestamp.DateTime);
            }
            else
            {
                await messageRepository.UpdateLastSpokeAsync(guildUser, message.Timestamp.DateTime);
            }
        }
    }
}
