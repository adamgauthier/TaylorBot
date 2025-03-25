using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet;

public class CommandServiceLogger(ILogger<CommandServiceLogger> logger, ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper)
{
    public Task OnCommandServiceLogAsync(LogMessage logMessage)
    {
        logger.Log(logSeverityToLogLevelMapper.MapFrom(logMessage.Severity), "CommandService: {Message}", logMessage.ToString(prependTimestamp: false));
        return Task.CompletedTask;
    }
}
