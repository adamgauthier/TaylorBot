using Microsoft.Extensions.Time.Testing;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Hosting;

internal sealed class ScenarioClock : TimeProvider
{
    private readonly FakeTimeProvider _clock = new(DateTimeOffset.UtcNow);
    private readonly Lock _lock = new();
    private readonly HashSet<ObservedTimer> _timers = [];
    private TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private long _version;

    public override DateTimeOffset GetUtcNow() => _clock.GetUtcNow();
    public override long GetTimestamp() => _clock.GetTimestamp();
    public override long TimestampFrequency => _clock.TimestampFrequency;
    public override TimeZoneInfo LocalTimeZone => _clock.LocalTimeZone;
    public void Advance(TimeSpan elapsed) => _clock.Advance(elapsed);

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
            return (_version, [.. _timers.Select(timer => timer.DueAt!.Value)]);
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
            }

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
