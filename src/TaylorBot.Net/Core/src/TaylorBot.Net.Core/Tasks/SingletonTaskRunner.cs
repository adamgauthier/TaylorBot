using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Tasks;

public class SingletonTaskRunner
{
    private readonly object _lockObject = new();
    private Task? _runningTask;

    private readonly ILogger<SingletonTaskRunner> _logger;
    private readonly TaskExceptionLogger _taskExceptionLogger;

    public SingletonTaskRunner(ILogger<SingletonTaskRunner> logger, TaskExceptionLogger taskExceptionLogger)
    {
        _logger = logger;
        _taskExceptionLogger = taskExceptionLogger;
    }

    public Task StartTaskIfNotStarted(Func<Task> action, string taskName)
    {
        if (_runningTask == null)
        {
            lock (_lockObject)
            {
                if (_runningTask == null)
                {
                    _runningTask = Task.Run(async () =>
                        await _taskExceptionLogger.LogOnError(action, taskName)
                    );
                }
                else
                {
                    _logger.LogWarning("Attempted to start task but it was already started.");
                }
            }
        }
        else
        {
            _logger.LogWarning("Attempted to start task but it was already started.");
        }

        return _runningTask;
    }
}
