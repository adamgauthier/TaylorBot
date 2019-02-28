using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Core.Tasks
{
    public class SingletonTaskRunner
    {
        private readonly object lockObject = new object();
        private Task runningTask;

        private readonly ILogger<SingletonTaskRunner> logger;

        public SingletonTaskRunner(ILogger<SingletonTaskRunner> logger)
        {
            this.logger = logger;
        }

        public Task StartTaskIfNotStarted(Action action)
        {
            if (runningTask == null)
            {
                lock (lockObject)
                {
                    if (runningTask == null)
                    {
                        runningTask = Task.Factory.StartNew(action);
                    }
                    else
                    {
                        logger.LogWarning(LogString.From("Attempted to start task but it was already started."));
                    }
                }
            }
            else
            {
                logger.LogWarning(LogString.From("Attempted to start task but it was already started."));
            }

            return runningTask;
        }
    }
}
