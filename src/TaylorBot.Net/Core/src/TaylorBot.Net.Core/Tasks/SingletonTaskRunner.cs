using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public partial class SingletonTaskRunner(ILogger<SingletonTaskRunner> logger, TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks)
{
    private readonly Lock _lockObject = new();
    private Task? _ranTask;

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to run {TaskName} but it was already ran.")]
    private partial void LogTaskAlreadyRan(string taskName);

    public Task RunTaskIfNotRan(Func<Task> action, string taskName)
    {
        return RunOnce(() => Task.Run(async () => await taskExceptionLogger.LogOnError(action, taskName)), taskName);
    }

    public Task RunFiniteTaskIfNotRan(Func<Task> action, string taskName)
    {
        return RunOnce(() => backgroundTasks.Queue(action, taskName), taskName);
    }

    private Task RunOnce(Func<Task> start, string taskName)
    {
        if (_ranTask == null)
        {
            lock (_lockObject)
            {
                if (_ranTask == null)
                {
                    _ranTask = start();
                }
                else
                {
                    LogTaskAlreadyRan(taskName);
                }
            }
        }
        else
        {
            LogTaskAlreadyRan(taskName);
        }

        return _ranTask;
    }
}
