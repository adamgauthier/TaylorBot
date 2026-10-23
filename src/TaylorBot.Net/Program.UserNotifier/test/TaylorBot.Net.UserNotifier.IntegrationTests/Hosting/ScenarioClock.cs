using Microsoft.Extensions.Time.Testing;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Hosting;

internal sealed class ScenarioClock : TimeProvider
{
    private readonly FakeTimeProvider _clock = new(DateTimeOffset.UtcNow);
    private readonly Lock _lock = new();
    private readonly HashSet<ObservedTimer> _timers = [];
    private TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private HashSet<ObservedTimer>? _deferredPeriodic;
    private long _version;

    public override DateTimeOffset GetUtcNow() => _clock.GetUtcNow();
    public override long GetTimestamp() => _clock.GetTimestamp();
    public override long TimestampFrequency => _clock.TimestampFrequency;
    public override TimeZoneInfo LocalTimeZone => _clock.LocalTimeZone;

    public async Task<bool> AdvanceAsync(TimeSpan elapsed, Func<Task> updateTransports)
    {
        lock (_lock)
        {
            if (_deferredPeriodic != null)
            {
                throw new InvalidOperationException("Concurrent scenario clock advances are not supported.");
            }
            _deferredPeriodic = [];
        }
        ObservedTimer[] deferred;
        try
        {
            _clock.Advance(elapsed);
            await updateTransports();
        }
        finally
        {
            lock (_lock)
            {
                deferred = [.. _deferredPeriodic!];
                _deferredPeriodic = null;
            }
            foreach (var timer in deferred)
            {
                timer.Fire();
            }
        }
        return deferred.Length != 0;
    }

    public async Task WaitForPeriodicTimersAsync(int count, CancellationToken cancellationToken)
    {
        while (true)
        {
            Task changed;
            lock (_lock)
            {
                if (_timers.Count(timer => timer.IsPeriodic) >= count)
                {
                    return;
                }
                changed = _changed.Task;
            }
            await changed.WaitAsync(cancellationToken);
        }
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        ObservedTimer timer = new(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    public (long Version, List<DateTimeOffset> DueTimes) GetStatus()
    {
        lock (_lock)
        {
            // Periodic infrastructure does not drive finite work to completion.
            return (_version, [.. _timers.Where(timer => !timer.IsPeriodic).Select(timer => timer.DueAt!.Value)]);
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

    private sealed class ObservedTimer : ITimer
    {
        private readonly ScenarioClock _owner;
        private readonly TimerCallback _callback;
        private readonly object? _state;
        private readonly ITimer _inner;
        private TimeSpan _period;
        private bool _disposed;

        public DateTimeOffset? DueAt { get; private set; }
        public bool IsPeriodic => _period > TimeSpan.Zero;

        public ObservedTimer(ScenarioClock owner, TimerCallback callback, object? state)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
            _inner = owner._clock.CreateTimer(Tick, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            lock (_owner._lock)
            {
                if (_disposed)
                {
                    return false;
                }

                _period = period;
                DueAt = dueTime == Timeout.InfiniteTimeSpan ? null : _owner.GetUtcNow() + dueTime;
                if (DueAt == null)
                {
                    _owner._timers.Remove(this);
                }
                else
                {
                    _owner._timers.Add(this);
                }

                var changed = _inner.Change(dueTime, period);
                _owner.Changed();

                return changed;
            }
        }

        private void Tick(object? state)
        {
            lock (_owner._lock)
            {
                DueAt = _period > TimeSpan.Zero ? _owner.GetUtcNow() + _period : null;
                if (DueAt == null)
                {
                    _owner._timers.Remove(this);
                }

                _owner.Changed();
                if (IsPeriodic && _owner._deferredPeriodic != null)
                {
                    _owner._deferredPeriodic.Add(this);
                    return;
                }
            }

            Fire();
        }

        public void Fire()
        {
            _callback(_state);
        }

        public void Dispose()
        {
            lock (_owner._lock)
            {
                _disposed = true;
                _owner._timers.Remove(this);
                _inner.Dispose();
                _owner.Changed();
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
