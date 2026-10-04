using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Birthday;

[Trait("Command", "birthday calendar")]
public sealed class BirthdayCalendarTests(DataServices data)
{
    [Fact]
    public async Task Calendar_OnlyIncludesPublicBirthdaysOfServerMembers()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(username: "PublicMember");
        var guild = await scenario.Given.GuildAsync(user);
        var privateMember = await scenario.Given.UserAsync(username: "PrivateMember");
        var outsider = await scenario.Given.UserAsync(username: "Outsider");
        await scenario.Given.MemberAsync(guild, privateMember);
        var upcoming = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).AddYears(-25);
        await scenario.Given.BirthdayAsync(user, upcoming);
        await scenario.Given.BirthdayAsync(privateMember, upcoming, isPrivate: true);
        await scenario.Given.BirthdayAsync(outsider, upcoming);
        await scenario.Given.RefreshBirthdayCalendarAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday calendar", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("PublicMember").And.NotContain("PrivateMember").And.NotContain("Outsider");
    }

    [Fact]
    public async Task Calendar_EmptyServerExplainsHowToAddBirthday()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday calendar", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("No upcoming birthdays").And.Contain("birthday set");
    }

    [Fact]
    public async Task Calendar_NextAndPreviousNavigatePersistedBirthdays()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayCalendarAsync(guild, count: 16);
        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday calendar", guild);
        first.Description.Should().Contain("BirthdayMember00").And.NotContain("BirthdayMember15");

        var next = await scenario.Discord.ClickAsync(user, first, "Next");

        next.Description.Should().Contain("BirthdayMember15").And.NotContain("BirthdayMember00");
        var previous = await scenario.Discord.ClickAsync(user, next, "Previous");
        previous.Description.Should().Contain("BirthdayMember00").And.NotContain("BirthdayMember15");
    }

    [Fact]
    public async Task Calendar_RequiresServerContext()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday calendar");

        response.ShouldBeError();
        response.Description.Should().Contain("server");
    }

    [Fact]
    public async Task Calendar_CancelDeletesCalendarMessage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday calendar", guild);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        response.ShouldBeDeleted();
    }
}
