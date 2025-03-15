using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public class SingletonTaskRunner(ILogger<SingletonTaskRunner> logger, TaskExceptionLogger taskExceptionLogger)
{
    private readonly Lock _lockObject = new();
    private Task? _runningTask;

    public Task StartTaskIfNotStarted(Func<Task> action, string taskName)
    {
        if (_runningTask == null)
        {
            lock (_lockObject)
            {
                if (_runningTask == null)
                {
                    _runningTask = Task.Run(async () =>
                        await taskExceptionLogger.LogOnError(action, taskName)
                    );
                }
                else
                {
                    logger.LogWarning("Attempted to start {TaskName} but it was already started.", taskName);
                }
            }
        }
        else
        {
            logger.LogWarning("Attempted to start {TaskName} but it was already started.", taskName);
        }

        return _runningTask;
    }
}
