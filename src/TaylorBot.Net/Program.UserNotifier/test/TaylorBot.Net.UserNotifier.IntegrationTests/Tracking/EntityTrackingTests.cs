namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class EntityTrackingTests(DataServices data)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartupTracking_SkipsGuildsNoLongerAvailable(bool unavailable)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?>
            {
                ["EntityTracker:UseRedisCache"] = "false",
                ["EntityTracker:TimeSpanBetweenGuildProcessedInReady"] = "00:00:03",
            });
        var owner = await scenario.Given.UserAsync();
        var first = await scenario.Given.GuildAsync(owner);
        var second = await scenario.Given.GuildAsync(owner);
        var third = await scenario.Given.GuildAsync(owner);

        await scenario.StartEntityTrackingUntilPacingAsync();
        foreach (var guild in new[] { first, second, third })
        {
            await scenario.RemoveGuildWhileTrackingAsync(guild.Id, unavailable);
            await scenario.Given.ForgetMembersAsync(guild);
        }
        await scenario.AdvanceAsync(TimeSpan.FromSeconds(3));

        (await scenario.State.MemberCountAsync(first)).Should().Be(0);
        (await scenario.State.MemberCountAsync(second)).Should().Be(0);
        (await scenario.State.MemberCountAsync(third)).Should().Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NewMember_IsNotIgnoredAndRetainsCreationTimeAfterRename(bool useRedisCache)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            settings: new Dictionary<string, string?> { ["EntityTracker:UseRedisCache"] = $"{useRedisCache}" });
        var owner = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        var user = scenario.Given.UntrackedUser();
        var before = DateTime.UtcNow;

        await scenario.Discord.JoinAsync(guild, user);
        var inserted = await scenario.State.UserTimestampsAsync(user);
        await scenario.Discord.RenameUserAsync(guild, user, "Renamed");

        inserted.IgnoreUntil.Should().BeNull();
        inserted.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        (await scenario.State.UserTimestampsAsync(user)).Should().Be(inserted);
        (await scenario.State.MemberAsync(guild, user)).Alive.Should().BeTrue();
        (await scenario.State.UsernamesAsync(user)).Should().Contain("Renamed");
    }

    [Fact]
    public async Task GuildAndUserChanges_KeepNameHistory()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user, name: "Before");

        await scenario.Discord.RenameGuildAsync(guild, "After");
        await scenario.Discord.RenameUserAsync(guild, user, "Renamed");

        (await scenario.State.GuildNamesAsync(guild)).Should().Contain(["Before", "After"]);
        (await scenario.State.UsernamesAsync(user)).Should().Contain("Renamed");
    }

    [Fact]
    public async Task NewTextChannel_IsPersisted()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var channelId = await scenario.Discord.CreateChannelAsync(guild, "New channel");

        (await scenario.State.ChannelExistsAsync(channelId)).Should().BeTrue();
    }

    [Fact]
    public async Task LeaveAndRejoin_UpdateMembership()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        await scenario.Discord.LeaveAsync(guild, user);
        (await scenario.State.MemberAsync(guild, user)).Alive.Should().BeFalse();
        await scenario.Discord.JoinAsync(guild, user);

        (await scenario.State.MemberAsync(guild, user)).Alive.Should().BeTrue();
    }
}
