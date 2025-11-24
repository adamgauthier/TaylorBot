using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public partial class TaskExceptionLogger(ILogger<TaskExceptionLogger> logger)
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in {TaskName}")]
    private partial void LogUnhandledException(Exception exception, string taskName);

    public async ValueTask LogOnError(ValueTask valueTask, string taskName)
    {
        try
        {
            await valueTask;
        }
        catch (Exception exception)
        {
            LogUnhandledException(exception, taskName);
            throw;
        }
    }

    public async Task LogOnError(Task task, string taskName)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            LogUnhandledException(exception, taskName);
            throw;
        }
    }

    public Task LogOnError(Func<Task> task, string taskName)
    {
        return LogOnError(task(), taskName);
    }
}
