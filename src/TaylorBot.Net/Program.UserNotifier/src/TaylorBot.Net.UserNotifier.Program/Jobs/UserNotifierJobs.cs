using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MessagesTracker.Domain;
using TaylorBot.Net.MinutesTracker.Domain;
using TaylorBot.Net.PatreonSync.Domain;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.Reminder.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.UserNotifier.Program.Options;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.UserNotifier.Program.Jobs;

public enum UserNotifierJob
{
    Minutes, LastSpoke, ChannelMessages, MemberMessages, Reddit, Youtube, Tumblr,
    BirthdayCalendar, Reminders, Patreon, BirthdayRoleAdd, BirthdayRoleRemove, BirthdayRewards,
}

public sealed record JobStatus(UserNotifierJob Name, long CompletedCycles, bool IsRunning, DateTimeOffset? NextRunAt);
public sealed record JobsStatus(long Version, IReadOnlyList<JobStatus> Jobs);

public sealed partial class UserNotifierJobs(
    IServiceProvider services,
    IOptionsMonitor<UserNotifierStartupOptions> startupOptions,
    BackgroundTasks backgroundTasks,
    TimeProvider timeProvider,
    ILogger<UserNotifierJobs> logger) : IHostedLifecycleService, IDisposable
{
    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly List<Job> _jobs = [];
    private TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private long _version;
    private bool _started;
    private bool _stopping;
    private bool _stopped;
    private bool _flushed;

    public void StartAfterReady()
    {
        lock (_lock)
        {
            if (_started || _stopping)
            {
                return;
            }

            _started = true;

            var options = startupOptions.CurrentValue;
            var messages = services.GetRequiredService<MessagesTrackerDomainService>();
            var roles = services.GetRequiredService<BirthdayRoleDomainService>();

            Add(UserNotifierJob.Minutes, services.GetRequiredService<MinutesTrackerDomainService>().RunMinutesAdderCycleAsync);

            Add(UserNotifierJob.LastSpoke, messages.RunPersistingLastSpokeCycleAsync);
            Add(UserNotifierJob.ChannelMessages, messages.RunPersistingTextChannelMessageCountCycleAsync);
            Add(UserNotifierJob.MemberMessages, messages.RunPersistingMemberMessagesAndWordsCycleAsync);

            Add(UserNotifierJob.Reddit, services.GetRequiredService<RedditNotifierService>().RunCheckingRedditsCycleAsync, options.RedditInitialDelay);
            Add(UserNotifierJob.Youtube, services.GetRequiredService<YoutubeNotifierService>().RunCheckingYoutubesCycleAsync, options.YoutubeInitialDelay);
            Add(UserNotifierJob.Tumblr, services.GetRequiredService<TumblrNotifierService>().RunCheckingTumblrsCycleAsync, options.TumblrInitialDelay);

            Add(UserNotifierJob.BirthdayCalendar, services.GetRequiredService<BirthdayCalendarDomainService>().RunRefreshingBirthdayCalendarCycleAsync, options.BirthdayCalendarInitialDelay);
            Add(UserNotifierJob.Reminders, services.GetRequiredService<ReminderNotifierDomainService>().RunCheckingRemindersCycleAsync, options.ReminderInitialDelay);
            Add(UserNotifierJob.Patreon, services.GetRequiredService<PatreonSyncDomainService>().RunSyncingPatreonSupportersCycleAsync, options.PatreonSyncInitialDelay);

            Add(UserNotifierJob.BirthdayRoleAdd, roles.RunAddingBirthdayRolesCycleAsync, options.BirthdayRoleAddInitialDelay);
            Add(UserNotifierJob.BirthdayRoleRemove, roles.RunRemovingBirthdayRolesCycleAsync, options.BirthdayRoleRemoveInitialDelay);
            Add(UserNotifierJob.BirthdayRewards, services.GetRequiredService<BirthdayRewardNotifierDomainService>().RunCheckingBirthdaysCycleAsync, options.BirthdayRewardInitialDelay);
        }
    }

    private void Add(UserNotifierJob name, Func<Task<TimeSpan>> cycle, TimeSpan initialDelay = default)
    {
        Job job = new(name);
        _jobs.Add(job);
        job.Completion = RunAsync(job, cycle, initialDelay);
    }

    private async Task RunAsync(Job job, Func<Task<TimeSpan>> cycle, TimeSpan delay)
    {
        var stopToken = _stop.Token;

        try
        {
            while (!stopToken.IsCancellationRequested)
            {
                Task wait;
                lock (_lock)
                {
                    wait = Task.Delay(delay, timeProvider, stopToken);
                    job.IsRunning = false;
                    job.NextRunAt = timeProvider.GetUtcNow() + delay;
                    Changed();
                }

                await wait;
                stopToken.ThrowIfCancellationRequested();

                lock (_lock)
                {
                    if (_stopping)
                    {
                        return;
                    }

                    job.IsRunning = true;
                    job.NextRunAt = null;
                    Changed();
                }

                await backgroundTasks.Run(async () => delay = await cycle(), $"{job.Name} cycle");

                lock (_lock)
                {
                    job.CompletedCycles++;
                }
            }
        }
        catch (OperationCanceledException exception) when (exception.CancellationToken == stopToken && stopToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            LogProducerFailed(exception, job.Name);
            throw;
        }
        finally
        {
            lock (_lock)
            {
                job.IsRunning = false;
                job.NextRunAt = null;
                Changed();
            }
        }
    }

    public JobsStatus GetStatus()
    {
        lock (_lock)
        {
            return new(_version, [.. _jobs.Select(job => new JobStatus(job.Name, job.CompletedCycles, job.IsRunning, job.NextRunAt))]);
        }
    }

    public Task WaitForChangeAsync(long version, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return version == _version ? _changed.Task.WaitAsync(cancellationToken) : Task.CompletedTask;
        }
    }

    private void Changed()
    {
        _version++;
        var previous = _changed;
        _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        previous.TrySetResult();
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        Task[] tasks;
        lock (_lock)
        {
            _stopping = true;
            tasks = [.. _jobs.Select(job => job.Completion)];
        }

        try
        {
            await _stop.CancelAsync();
            await Task.WhenAll(tasks).WaitAsync(cancellationToken);

            _stopped = true;
        }
        catch (Exception exception)
        {
            LogShutdownIncomplete(exception);
            throw;
        }
    }

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        if (!_stopped)
        {
            throw new InvalidOperationException("Cannot flush tracking data while scheduled jobs may still be running.");
        }

        if (_flushed)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await backgroundTasks.DrainAsync(cancellationToken);

        await backgroundTasks.Run(async () =>
        {
            await services.GetRequiredService<IGuildUserLastSpokeRepository>().PersistQueuedLastSpokeUpdatesAsync();
            await services.GetRequiredService<ITextChannelMessageCountRepository>().PersistQueuedMessageCountIncrementsAsync();
            await services.GetRequiredService<IMessageRepository>().PersistQueuedMessagesAndWordsAsync();
        }, "Final tracking flush").WaitAsync(cancellationToken);

        _flushed = true;
    }

    public void Dispose() => _stop.Dispose();

    [LoggerMessage(Level = LogLevel.Error, Message = "Scheduled producer {Job} stopped unexpectedly")]
    private partial void LogProducerFailed(Exception exception, UserNotifierJob job);

    [LoggerMessage(Level = LogLevel.Error, Message = "Scheduled shutdown did not finish; active work has not been abandoned or flushed")]
    private partial void LogShutdownIncomplete(Exception exception);

    private sealed class Job(UserNotifierJob name)
    {
        public UserNotifierJob Name { get; } = name;
        public Task Completion { get; set; } = Task.CompletedTask;
        public long CompletedCycles { get; set; }
        public bool IsRunning { get; set; }
        public DateTimeOffset? NextRunAt { get; set; }
    }
}
