﻿using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public class TaskExceptionLogger(ILogger<TaskExceptionLogger> logger)
{
    public async ValueTask LogOnError(ValueTask valueTask, string taskName)
    {
        try
        {
            await valueTask;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception in {TaskName}", taskName);
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
            logger.LogError(exception, "Unhandled exception in {TaskName}", taskName);
            throw;
        }
    }

    public Task LogOnError(Func<Task> task, string taskName)
    {
        return LogOnError(task(), taskName);
    }
}
