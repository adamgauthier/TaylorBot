using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Reminders;

[Trait("Command", "remind add")]
[Trait("Command", "remind manage")]
public sealed class ReminderTests(DataServices data)
{
    [Fact]
    public async Task Add_PersistsTextAndRequestedDelay()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var before = DateTime.UtcNow;

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", "2h"), SlashArgument.Text("text", "Listen to the album")]);

        response.ShouldBeSuccess();
        var reminder = (await scenario.State.RemindersAsync(user)).Should().ContainSingle().Which;
        reminder.Text.Should().Be("Listen to the album");
        reminder.RemindAt.Should().BeOnOrAfter(before.AddHours(2)).And.BeOnOrBefore(DateTime.UtcNow.AddHours(2));
    }

    [Theory]
    [InlineData("0m", "less than")]
    [InlineData("366d", "more than")]
    [InlineData("not a duration", "Unrecognized format")]
    public async Task Add_InvalidDurationDoesNotPersist(string duration, string reason)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", duration), SlashArgument.Text("text", "Listen to the album")]);

        response.ShouldBeError();
        response.Description.Should().Contain(reason);
        (await scenario.State.RemindersAsync(user)).Should().BeEmpty();
    }

    [Fact]
    public async Task Add_ExcessivelyLongTextDoesNotPersist()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", "1h"), SlashArgument.Text("text", new string('x', count: 4097))]);

        response.ShouldBeError();
        response.Description.Should().Contain("longer than");
        (await scenario.State.RemindersAsync(user)).Should().BeEmpty();
    }

    [Fact]
    public async Task Add_StandardAccountCannotExceedTwoReminders()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.ReminderAsync(user, "First");
        await scenario.Given.ReminderAsync(user, "Second");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", "1h"), SlashArgument.Text("text", "Third")]);

        response.ShouldBeError();
        response.Description.Should().Contain("more than 2");
        (await scenario.State.RemindersAsync(user)).Select(reminder => reminder.Text).Should().BeEquivalentTo(["First", "Second"]);
    }

    [Fact]
    public async Task Add_PlusAccountCanAddThirdReminder()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.PlusAsync(user);
        await scenario.Given.ReminderAsync(user, "First");
        await scenario.Given.ReminderAsync(user, "Second");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", "1h"), SlashArgument.Text("text", "Third")]);

        response.ShouldBeSuccess();
        (await scenario.State.RemindersAsync(user)).Select(reminder => reminder.Text).Should().BeEquivalentTo(["First", "Second", "Third"]);
    }

    [Fact]
    public async Task Add_PlusAccountCannotExceedFourReminders()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.PlusAsync(user);
        await scenario.Given.ReminderAsync(user, "First");
        await scenario.Given.ReminderAsync(user, "Second");
        await scenario.Given.ReminderAsync(user, "Third");
        await scenario.Given.ReminderAsync(user, "Fourth");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind add",
            arguments: [SlashArgument.Text("time", "1h"), SlashArgument.Text("text", "Fifth")]);

        response.ShouldBeError();
        response.Description.Should().Contain("more than 4");
        (await scenario.State.RemindersAsync(user)).Should().HaveCount(4);
    }

    [Fact]
    public async Task Manage_EmptyListOffersAddCommandWithoutClearControls()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "remind manage");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("don't have any reminders").And.Contain("remind add");
        response.Message.GetProperty("components").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Manage_ClearOnePreservesRemindersAddedAfterPrompt()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.ReminderAsync(user, "First");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "remind manage");
        prompt.Description.Should().Contain("First");
        await scenario.Given.ReminderAsync(user, "Second");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Clear #1");

        response.ShouldBeSuccess();
        (await scenario.State.RemindersAsync(user)).Should().ContainSingle().Which.Text.Should().Be("Second");
    }

    [Fact]
    public async Task Manage_ClearAllPreservesOtherUsersReminders()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.ReminderAsync(user, "First");
        await scenario.Given.ReminderAsync(user, "Second");
        await scenario.Given.ReminderAsync(other, "Unchanged");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "remind manage");

        var response = await scenario.Discord.ClickAsync(user, prompt, "Clear all");

        response.ShouldBeSuccess();
        (await scenario.State.RemindersAsync(user)).Should().BeEmpty();
        (await scenario.State.RemindersAsync(other)).Should().ContainSingle().Which.Text.Should().Be("Unchanged");
    }

    [Theory]
    [InlineData("Clear #1")]
    [InlineData("Clear all")]
    public async Task Manage_AnotherUserCannotClearReminders(string button)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.ReminderAsync(user, "Unchanged");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "remind manage");

        var response = await scenario.Discord.ClickAsync(other, prompt, button);

        response.Requests.Should().ContainSingle("another user's click is acknowledged without editing the original message");
        (await scenario.State.RemindersAsync(user)).Should().ContainSingle().Which.Text.Should().Be("Unchanged");
    }
}
