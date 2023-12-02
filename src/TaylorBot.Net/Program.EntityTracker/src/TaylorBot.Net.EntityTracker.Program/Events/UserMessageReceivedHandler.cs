using Discord.WebSocket;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessagesTracker.Domain;

namespace TaylorBot.Net.EntityTracker.Program.Events;

public class UserMessageReceivedHandler : IUserMessageReceivedHandler
{
    private readonly TaskExceptionLogger _taskExceptionLogger;
    private readonly MessagesTrackerDomainService _messagesTrackerDomainService;

    public UserMessageReceivedHandler(TaskExceptionLogger taskExceptionLogger, MessagesTrackerDomainService messagesTrackerDomainService)
    {
        _taskExceptionLogger = taskExceptionLogger;
        _messagesTrackerDomainService = messagesTrackerDomainService;
    }

    public Task UserMessageReceivedAsync(SocketUserMessage userMessage)
    {
        if (userMessage.Channel is SocketTextChannel textChannel && userMessage.Author is SocketGuildUser guildUser)
        {
            _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
                _messagesTrackerDomainService.OnGuildUserMessageReceivedAsync(textChannel, guildUser, userMessage),
                nameof(_messagesTrackerDomainService.OnGuildUserMessageReceivedAsync)
            ));
        }

        return Task.CompletedTask;
    }
}
