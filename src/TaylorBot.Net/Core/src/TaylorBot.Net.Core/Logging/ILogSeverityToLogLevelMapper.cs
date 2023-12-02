using Discord;
using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Logging;

public interface ILogSeverityToLogLevelMapper
{
    LogLevel MapFrom(LogSeverity logSeverity);
}
