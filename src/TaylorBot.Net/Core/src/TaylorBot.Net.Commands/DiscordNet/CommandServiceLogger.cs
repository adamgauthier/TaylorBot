using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet;

public partial class CommandServiceLogger(ILogger<CommandServiceLogger> logger, ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper)
{
    public Task OnCommandServiceLogAsync(LogMessage logMessage)
    {
        LogCommandServiceMessage(logSeverityToLogLevelMapper.MapFrom(logMessage.Severity), logMessage.ToString(prependTimestamp: false));
        return Task.CompletedTask;
    }

    [LoggerMessage(Message = "CommandService: {Message}")]
    private partial void LogCommandServiceMessage(LogLevel logLevel, string message);
}
