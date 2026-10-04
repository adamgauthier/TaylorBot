using Microsoft.Extensions.Logging;

namespace TaylorBot.Net.Core.Tasks;

public sealed partial class BackgroundTasks(ILogger<BackgroundTasks> logger)
{
    private readonly Lock _lock = new();
    private readonly Dictionary<Task, string> _pending = [];

    public Task Queue(Func<Task> action, string name) => Run(() => Task.Run(action), name);

    public Task Run(Func<Task> action, string name)
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource finished = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_lock)
        {
            _pending.Add(finished.Task, name);
        }
        _ = ExecuteAsync(action, name, completion, finished);
        return completion.Task;
    }

    private async Task ExecuteAsync(Func<Task> action, string name, TaskCompletionSource completion, TaskCompletionSource finished)
    {
        try
        {
            await action();
            completion.SetResult();
        }
        catch (Exception exception)
        {
            LogFailed(exception, name);
            if (exception is OperationCanceledException cancelled)
            {
                completion.SetCanceled(cancelled.CancellationToken);
            }
            else
            {
                completion.SetException(exception);
                // Detached callers still get logged failures without an unobserved exception.
                _ = completion.Task.Exception;
            }
        }
        finally
        {
            lock (_lock)
            {
                _pending.Remove(finished.Task);
                finished.SetResult();
            }
        }
    }

    public async Task DrainAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                Task[] pending;
                lock (_lock)
                {
                    pending = [.. _pending.Keys];
                }
                if (pending.Length == 0)
                {
                    return;
                }
                // Failures are logged and returned to callers, but must not abandon other work.
                await Task.WhenAll(pending).WaitAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            string names;
            lock (_lock)
            {
                names = string.Join(", ", _pending.Values);
            }
            LogDrainCancelled(names);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Background task {TaskName} failed")]
    private partial void LogFailed(Exception exception, string taskName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Stopped waiting for background tasks: {TaskNames}. Work has not been cancelled.")]
    private partial void LogDrainCancelled(string taskNames);
}
