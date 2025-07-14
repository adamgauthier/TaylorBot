using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public class SingletonTaskRunner(ILogger<SingletonTaskRunner> logger, TaskExceptionLogger taskExceptionLogger)
{
    private readonly Lock _lockObject = new();
    private Task? _ranTask;

    public Task RunTaskIfNotRan(Func<Task> action, string taskName)
    {
        if (_ranTask == null)
        {
            lock (_lockObject)
            {
                if (_ranTask == null)
                {
                    _ranTask = Task.Run(async () =>
                        await taskExceptionLogger.LogOnError(action, taskName)
                    );
                }
                else
                {
                    logger.LogWarning("Attempted to run {TaskName} but it was already ran.", taskName);
                }
            }
        }
        else
        {
            logger.LogWarning("Attempted to run {TaskName} but it was already ran.", taskName);
        }

        return _ranTask;
    }
}
