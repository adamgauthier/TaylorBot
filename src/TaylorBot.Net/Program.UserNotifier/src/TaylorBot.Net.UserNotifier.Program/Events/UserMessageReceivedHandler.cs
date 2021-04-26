using Discord.WebSocket;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.MessageLogging.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Events
{
    public class UserMessageReceivedHandler : IUserMessageReceivedHandler
    {
        private readonly MessageDeletedLoggerService _messageDeletedLoggerService;

        public UserMessageReceivedHandler(MessageDeletedLoggerService messageDeletedLoggerService)
        {
            _messageDeletedLoggerService = messageDeletedLoggerService;
        }

        public async Task UserMessageReceivedAsync(SocketUserMessage userMessage)
        {
            if (userMessage.Channel is SocketTextChannel textChannel)
            {
                await _messageDeletedLoggerService.OnGuildUserMessageReceivedAsync(textChannel, userMessage);
            }
        }
    }
}
