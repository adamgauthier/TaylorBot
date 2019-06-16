using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.MessagesTracker.Domain
{
    public class MessagesTrackerDomainService
    {
        private readonly IMessageRepository messageRepository;
        private readonly WordCounter wordCounter;
        private readonly TaylorBotClient taylorBotClient;

        public MessagesTrackerDomainService(IMessageRepository messageRepository, WordCounter wordCounter, TaylorBotClient taylorBotClient)
        {
            this.messageRepository = messageRepository;
            this.wordCounter = wordCounter;
            this.taylorBotClient = taylorBotClient;
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
