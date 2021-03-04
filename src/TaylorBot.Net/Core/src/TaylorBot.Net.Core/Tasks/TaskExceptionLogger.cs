using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Tasks
{
    public class TaskExceptionLogger
    {
        public ILogger<TaskExceptionLogger> logger;

        public TaskExceptionLogger(ILogger<TaskExceptionLogger> logger)
        {
            this.logger = logger;
        }

        public async Task LogOnError(Task task, string taskName, bool shouldRethrow = true)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Unhandled exception in {taskName}{(shouldRethrow ? "" : ", not rethrowing")}.");
                if (shouldRethrow)
                    throw;
            }
        }

        public Task LogOnError(Func<Task> task, string taskName, bool shouldRethrow = true)
        {
            return LogOnError(task(), taskName, shouldRethrow);
        }
    }
}
