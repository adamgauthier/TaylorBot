using Discord;
using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Logging
{
    public class LogSeverityToLogLevelMapper : ILogSeverityToLogLevelMapper
    {
        public LogLevel MapFrom(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Verbose:
                    return LogLevel.Trace;
                case LogSeverity.Debug:
                    return LogLevel.Debug;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logSeverity));
            }
        }
    }
}
