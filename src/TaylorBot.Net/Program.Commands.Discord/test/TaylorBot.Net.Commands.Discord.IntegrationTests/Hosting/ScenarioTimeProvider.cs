namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;

// Move application UTC time without changing real transport deadlines and timers.
public sealed class ScenarioTimeProvider : TimeProvider
{
    private long _ticks = DateTimeOffset.UtcNow.Ticks;

    public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref _ticks), TimeSpan.Zero);

    public void Advance(TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);
        Interlocked.Add(ref _ticks, elapsed.Ticks);
    }
}
