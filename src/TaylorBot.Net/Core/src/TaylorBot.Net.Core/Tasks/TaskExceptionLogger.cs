using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Tasks;

public class TaskExceptionLogger
{
    public ILogger<TaskExceptionLogger> _logger;

    public TaskExceptionLogger(ILogger<TaskExceptionLogger> logger)
    {
        _logger = logger;
    }

    public async ValueTask LogOnError(ValueTask valueTask, string taskName)
    {
        try
        {
            await valueTask;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception in {TaskName}", taskName);
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
            _logger.LogError(exception, "Unhandled exception in {TaskName}", taskName);
            throw;
        }
    }

    public Task LogOnError(Func<Task> task, string taskName)
    {
        return LogOnError(task(), taskName);
    }
}
