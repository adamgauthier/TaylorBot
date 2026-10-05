using System.Net;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class BirthdayTests(DataServices data)
{
    [Fact]
    public async Task EligibleBirthday_RewardsAndNotifiesOnlyOnce()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var user = await scenario.Given.UserAsync(taypoints: 100);
        await scenario.Given.BirthdayAsync(user);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRewards);
        var repeated = await scenario.RunJobAsync(UserNotifierJob.BirthdayRewards);

        output.Text.Should().Contain("1,989").And.Contain("2,089");
        repeated.Messages.Should().BeEmpty();
        (await scenario.State.TaypointsAsync(user)).Should().Be(2089);
        (await scenario.State.BirthdayRewardRecordedAsync(user)).Should().BeTrue();
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(0, true)]
    public async Task IneligibleBirthday_DoesNotReward(int daysAgo, bool rewarded)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user, daysAgo, rewarded: rewarded);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRewards);

        output.Messages.Should().BeEmpty();
        (await scenario.State.TaypointsAsync(user)).Should().Be(0);
    }

    [Fact]
    public async Task UnresolvedRecipient_LogsFailureAndContinuesNotifying()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var missing = await scenario.Given.UserAsync(username: "Unavailable");
        var available = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(missing);
        await scenario.Given.BirthdayAsync(available);
        scenario.DiscordApi.Resource($"users/{missing.Id}", new { code = 10013, message = "Unknown User" }, HttpStatusCode.NotFound);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(available.Id));
        scenario.Logs.ExpectError<BirthdayRewardNotifierDomainService>("about their birthday reward");

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRewards);

        output.Messages.Should().ContainSingle();
        (await scenario.State.TaypointsAsync(missing)).Should().Be(1989);
        (await scenario.State.TaypointsAsync(available)).Should().Be(1989);
    }

    [Fact]
    public async Task RejectedDm_DoesNotUndoBirthdayReward()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRewards);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user);
        scenario.DiscordApi.RejectMessage(NotifierDiscordApi.DmChannel(user.Id));

        await scenario.RunJobAsync(UserNotifierJob.BirthdayRewards);

        (await scenario.State.TaypointsAsync(user)).Should().Be(1989);
        (await scenario.State.BirthdayRewardRecordedAsync(user)).Should().BeTrue();
    }

    [Fact]
    public async Task CalendarRefresh_ExcludesPrivateBirthdays()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayCalendar);
        var publicUser = await scenario.Given.UserAsync();
        var privateUser = await scenario.Given.UserAsync(username: "Private");
        await scenario.Given.BirthdayAsync(publicUser);
        await scenario.Given.BirthdayAsync(privateUser, isPrivate: true);
        (await scenario.State.CalendarUsersAsync()).Should().BeEmpty();

        await scenario.RunJobAsync(UserNotifierJob.BirthdayCalendar);

        (await scenario.State.CalendarUsersAsync()).Should().Equal(publicUser.Id);
    }

    [Fact]
    public async Task BirthdayRole_AddsRoleAndRecordsHistory()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user);
        var role = await scenario.Given.BirthdayRoleAsync(guild);
        await scenario.Given.ModLogAsync(guild);
        scenario.DiscordApi.Expect("PUT", $"guilds/{guild.Id}/members/{user.Id}/roles/{role}");

        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);
        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExpiredBirthdayRole_RecordsRemovalEvenIfAlreadyAbsent(bool hasRole)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleRemove);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await scenario.Given.BirthdayRoleAsync(guild);
        await scenario.Given.BirthdayRoleGivenAsync(guild, user, role, hasRole: hasRole);
        await scenario.Given.ModLogAsync(guild);
        if (hasRole)
        {
            scenario.DiscordApi.Expect("DELETE", $"guilds/{guild.Id}/members/{user.Id}/roles/{role}");
        }

        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleRemove);

        (await scenario.State.BirthdayRoleRemovedAsync(guild, user)).Should().BeTrue();
    }

    [Fact]
    public async Task MissingBirthdayRole_DoesNotRecordRoleGiven()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user);
        await scenario.Given.BirthdayRoleAsync(guild, exists: false);

        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
    }

    [Fact]
    public async Task UnexpiredBirthdayRole_IsNotRemoved()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleRemove);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await scenario.Given.BirthdayRoleAsync(guild);
        await scenario.Given.BirthdayRoleGivenAsync(guild, user, role, expired: false);

        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleRemove);

        (await scenario.State.BirthdayRoleRemovedAsync(guild, user)).Should().BeFalse();
    }

    [Fact]
    public async Task FailedCalendarRefresh_UsesFailureBackoffThenResumesNormalCadence()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayCalendar);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user);
        await using var failure = await scenario.Given.UnavailableBirthdayCalendarAsync();
        scenario.Logs.ExpectError<BirthdayCalendarDomainService>("Unhandled exception in RefreshBirthdayCalendarAsync.");

        await scenario.RunJobAsync(UserNotifierJob.BirthdayCalendar);
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.BirthdayCalendar).NextRunAt.Should().Be(scenario.Now + TimeSpan.FromSeconds(30));
        await failure.DisposeAsync();
        await scenario.RunJobAsync(UserNotifierJob.BirthdayCalendar);

        (await scenario.State.CalendarUsersAsync()).Should().Equal(user.Id);
        scenario.Jobs.Jobs.Single(job => job.Name == UserNotifierJob.BirthdayCalendar).NextRunAt.Should().Be(scenario.Now + TimeSpan.FromHours(12));
    }
}
