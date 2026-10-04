using TaylorBot.Net.UserNotifier.Program.Jobs;
using TaylorBot.Net.MinutesTracker.Domain;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class MinutesTrackingTests(DataServices data)
{
    [Fact]
    public async Task ActiveMember_ReceivesMinutesAndSixthCyclePoint()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ActiveMemberAsync(guild, user);

        await scenario.RunCyclesAsync(UserNotifierJob.Minutes, count: 5);

        var member = await scenario.State.MemberAsync(guild, user);
        member.Minutes.Should().Be(5);
        member.Experience.Should().Be(1);
        (await scenario.State.TaypointsAsync(user)).Should().Be(1);
    }

    [Fact]
    public async Task InactiveMember_DoesNotReceiveMinutesOrPoints()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ActiveMemberAsync(guild, user, active: false);

        await scenario.RunJobAsync(UserNotifierJob.Minutes);

        (await scenario.State.MemberAsync(guild, user)).Minutes.Should().Be(0);
        (await scenario.State.TaypointsAsync(user)).Should().Be(0);
    }

    [Fact]
    public async Task FailedMinute_DoesNotAdvancePointCadence()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ActiveMemberAsync(guild, user);
        await using var failure = await scenario.Given.RejectTrackingWritesAsync(UserNotifierJob.Minutes);
        scenario.Logs.ExpectError<MinutesTrackerDomainService>("Exception occurred when attempting to add minutes");

        await scenario.RunJobAsync(UserNotifierJob.Minutes);
        await failure.DisposeAsync();
        await scenario.RunCyclesAsync(UserNotifierJob.Minutes, count: 4);
        (await scenario.State.TaypointsAsync(user)).Should().Be(0);
        await scenario.RunJobAsync(UserNotifierJob.Minutes);

        (await scenario.State.MemberAsync(guild, user)).Minutes.Should().Be(5);
        (await scenario.State.TaypointsAsync(user)).Should().Be(1);
    }
}
