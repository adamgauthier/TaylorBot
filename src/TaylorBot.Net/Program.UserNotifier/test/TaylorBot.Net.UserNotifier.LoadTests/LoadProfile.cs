namespace TaylorBot.Net.UserNotifier.LoadTests;

public sealed record LoadProfile
{
    public int Shards { get; init; } = 5;
    public int Guilds { get; init; } = 5430;
    public int ChannelsPerGuild { get; init; } = 20;
    public int CachedMembersPerGuild { get; init; } = 20;
    public int RolesPerGuild { get; init; } = 10;
    public int UserPools { get; init; } = 1000;
    public int MessageCharacters { get; init; } = 200;
    public int EventsPerSecond { get; init; } = 20;
    public int SteadySeconds { get; init; } = 120;
    public int BurstSeconds { get; init; } = 30;
    public int BurstMultiplier { get; init; } = 5;
    public int CooldownSeconds { get; init; } = 30;
    public int StartupGuildDelayMilliseconds { get; init; } = 3000;
    public int RestDelayMilliseconds { get; init; }
    public bool Reconnect { get; init; } = true;
    public bool FreshSession { get; init; } = true;
    public bool ModerationLogging { get; init; } = true;
    public int MessageCacheSize { get; init; } = 3;
    public int DeadlineSeconds { get; init; } = 4200;
    public int Seed { get; init; } = 1989;

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(Shards, other: 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Shards, other: 16);
        ArgumentOutOfRangeException.ThrowIfLessThan(Guilds, Shards);
        ArgumentOutOfRangeException.ThrowIfLessThan(ChannelsPerGuild, other: 2);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ChannelsPerGuild, other: 500);
        ArgumentOutOfRangeException.ThrowIfLessThan(CachedMembersPerGuild, other: 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(RolesPerGuild, other: 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(RolesPerGuild, other: 250);
        ArgumentOutOfRangeException.ThrowIfLessThan(UserPools, other: 1);
        ArgumentOutOfRangeException.ThrowIfNegative(MessageCharacters);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(MessageCharacters, other: 2000);
        ArgumentOutOfRangeException.ThrowIfLessThan(EventsPerSecond, other: 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(SteadySeconds, other: 1);
        ArgumentOutOfRangeException.ThrowIfNegative(BurstSeconds);
        ArgumentOutOfRangeException.ThrowIfLessThan(BurstMultiplier, other: 1);
        ArgumentOutOfRangeException.ThrowIfNegative(CooldownSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(StartupGuildDelayMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(RestDelayMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(MessageCacheSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(DeadlineSeconds, other: 60);
        _ = checked(EventsPerSecond * BurstMultiplier);
    }
}

public sealed record LoadStatus(string Phase, bool Finished, string? Failure);
public sealed record WorkerSample(
    DateTimeOffset At, long WorkingSet, long PrivateBytes, long ManagedBytes, long CommittedBytes, long FragmentedBytes,
    long AllocatedBytes, long AvailableManagedBytes, int Gen0, int Gen1, int Gen2, long PauseTicks, int PendingTasks, int ReadyShards,
    int Guilds, long CachedMembers, long Channels, long CachedMessages, long ReceivedMessages, long ReadyEvents, long Errors,
    string? CgroupCurrent, string? CgroupPeak, string? CgroupEvents, string? CgroupCpu, bool ServerGc,
    int ThreadPoolThreads, long ThreadPoolPending, long ThreadPoolCompleted, double SamplingMilliseconds, long SamplingAllocatedBytes);
