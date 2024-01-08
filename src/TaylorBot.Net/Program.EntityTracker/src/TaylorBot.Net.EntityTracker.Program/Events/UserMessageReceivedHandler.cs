using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class UserMessageReceivedHandler(TaskExceptionLogger taskExceptionLogger, MessagesTrackerDomainService messagesTrackerDomainService) : IUserMessageReceivedHandler
{
    public Task UserMessageReceivedAsync(SocketUserMessage userMessage)
    {
        if (userMessage.Channel is SocketTextChannel textChannel && userMessage.Author is SocketGuildUser guildUser)
        {
            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                messagesTrackerDomainService.OnGuildUserMessageReceivedAsync(textChannel, guildUser, userMessage),
                nameof(messagesTrackerDomainService.OnGuildUserMessageReceivedAsync)
            ));
        }

        return Task.CompletedTask;
    }
}
