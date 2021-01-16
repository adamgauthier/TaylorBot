using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.StatsTracker.Program.Events
{
    public class UserMessageReceivedHandler : IUserMessageReceivedHandler
    {
        private readonly MessagesTrackerDomainService _messagesTrackerDomainService;

        public UserMessageReceivedHandler(MessagesTrackerDomainService messagesTrackerDomainService)
        {
            _messagesTrackerDomainService = messagesTrackerDomainService;
        }

        public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
        {
            if (userMessage.Channel is SocketTextChannel textChannel && userMessage.Author is SocketGuildUser guildUser)
            {
                await _messagesTrackerDomainService.OnGuildUserMessageReceivedAsync(textChannel, guildUser, userMessage);
            }
        }
    }
}
