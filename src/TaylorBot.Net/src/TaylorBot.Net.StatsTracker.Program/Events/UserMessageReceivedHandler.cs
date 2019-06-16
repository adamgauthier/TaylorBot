using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Client;
using Discord.WebSocket;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class UserMessageReceivedHandler : IUserMessageReceivedHandler
    {
        private readonly TaylorBotClient taylorBotClient;
        private readonly MessagesTrackerDomainService messagesTrackerDomainService;

        public UserMessageReceivedHandler(TaylorBotClient taylorBotClient, MessagesTrackerDomainService messagesTrackerDomainService)
        {
            this.taylorBotClient = taylorBotClient;
            this.messagesTrackerDomainService = messagesTrackerDomainService;
        }

        public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
        {
            if (userMessage.Channel is SocketTextChannel textChannel && userMessage.Author is SocketGuildUser guildUser)
            {
                await messagesTrackerDomainService.OnGuildUserMessageReceivedAsync(textChannel, guildUser, userMessage);
            }
        }
    }
}
