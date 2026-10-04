using TaylorBot.Net.UserNotifier.IntegrationTests.Discord;
using TaylorBot.Net.UserNotifier.Program.Jobs;
using TaylorBot.Net.Reminder.Domain;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class ReminderTests(DataServices data)
{
    [Fact]
    public async Task DueReminder_SendsDmAndRemovesReminder()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        var user = await scenario.Given.UserAsync();
        var reminder = await scenario.Given.ReminderAsync(user, text: "Remember the tea");
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(user.Id));

        var output = await scenario.RunJobAsync(UserNotifierJob.Reminders);

        output.Messages.Should().ContainSingle();
        output.Text.Should().Contain("Remember the tea");
        (await scenario.State.ReminderExistsAsync(reminder)).Should().BeFalse();
    }

    [Fact]
    public async Task FutureReminder_IsNotSentOrRemoved()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        var user = await scenario.Given.UserAsync();
        var reminder = await scenario.Given.ReminderAsync(user, due: false);

        var output = await scenario.RunJobAsync(UserNotifierJob.Reminders);

        output.Messages.Should().BeEmpty();
        (await scenario.State.ReminderExistsAsync(reminder)).Should().BeTrue();
    }

    [Theory]
    [InlineData(50007)]
    [InlineData(50278)]
    public async Task UndeliverableDm_RemovesReminder(int errorCode)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        var user = await scenario.Given.UserAsync();
        var reminder = await scenario.Given.ReminderAsync(user);
        scenario.DiscordApi.RejectMessage(NotifierDiscordApi.DmChannel(user.Id), code: errorCode);

        await scenario.RunJobAsync(UserNotifierJob.Reminders);

        (await scenario.State.ReminderExistsAsync(reminder)).Should().BeFalse();
    }

    [Fact]
    public async Task OtherDiscordFailure_RetainsReminderAndContinuesWithOtherUsers()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.Reminders);
        var rejected = await scenario.Given.UserAsync();
        var available = await scenario.Given.UserAsync(username: "Available");
        var retained = await scenario.Given.ReminderAsync(rejected);
        var delivered = await scenario.Given.ReminderAsync(available);
        scenario.DiscordApi.RejectMessage(NotifierDiscordApi.DmChannel(rejected.Id), code: 50013);
        scenario.DiscordApi.ExpectMessage(NotifierDiscordApi.DmChannel(available.Id));
        scenario.Logs.ExpectError<ReminderNotifierDomainService>("Exception occurred when attempting to notify");

        await scenario.RunJobAsync(UserNotifierJob.Reminders);

        (await scenario.State.ReminderExistsAsync(retained)).Should().BeTrue();
        (await scenario.State.ReminderExistsAsync(delivered)).Should().BeFalse();
    }
}
