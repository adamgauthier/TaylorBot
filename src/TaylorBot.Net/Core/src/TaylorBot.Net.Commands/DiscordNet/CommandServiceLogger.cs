using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet
{
    public class CommandServiceLogger
    {
        private readonly ILogger<CommandServiceLogger> _logger;
        private readonly ILogSeverityToLogLevelMapper _logSeverityToLogLevelMapper;

        public CommandServiceLogger(ILogger<CommandServiceLogger> logger, ILogSeverityToLogLevelMapper logSeverityToLogLevelMapper)
        {
            _logger = logger;
            _logSeverityToLogLevelMapper = logSeverityToLogLevelMapper;
        }

        public Task OnCommandServiceLogAsync(LogMessage logMessage)
        {
            _logger.Log(_logSeverityToLogLevelMapper.MapFrom(logMessage.Severity), logMessage.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }
    }
}
