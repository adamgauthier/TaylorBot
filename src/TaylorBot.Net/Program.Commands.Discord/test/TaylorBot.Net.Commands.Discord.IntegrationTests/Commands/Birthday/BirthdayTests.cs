using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Birthday;

[Trait("Command", "birthday show")]
[Trait("Command", "birthday set")]
[Trait("Command", "birthday age")]
[Trait("Command", "birthday clear")]
[Trait("Command", "birthday horoscope")]
public sealed class BirthdayTests(DataServices data)
{
    [Fact]
    public async Task Set_PersistsDateAndPrivacy()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set", arguments:
        [
            SlashArgument.Integer("day", value: 13),
            SlashArgument.Integer("month", value: 12),
            SlashArgument.Integer("year", value: 1989),
            SlashArgument.Boolean("privately", value: true),
        ]);

        response.ShouldBeSuccess();
        var birthday = await scenario.State.BirthdayAsync(user);
        birthday!.Birthday.Should().Be(new DateOnly(year: 1989, month: 12, day: 13));
        birthday.IsPrivate.Should().BeTrue();
    }

    [Fact]
    public async Task Set_WithoutYearAllowsLeapDay()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set",
            arguments: [SlashArgument.Integer("day", value: 29), SlashArgument.Integer("month", value: 2)]);

        response.ShouldBeSuccess();
        (await scenario.State.BirthdayAsync(user))!.Birthday.Should().Be(new DateOnly(year: 1804, month: 2, day: 29));
    }

    [Theory]
    [InlineData(31, 2, 2000, "not valid")]
    [InlineData(1, 1, 2025, "higher or equal to 13")]
    [InlineData(1, 1, 1800, "lower or equal to 115")]
    public async Task Set_InvalidDateOrAgeDoesNotPersist(int day, int month, int year, string reason)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set", arguments:
        [
            SlashArgument.Integer("day", day),
            SlashArgument.Integer("month", month),
            SlashArgument.Integer("year", year),
        ]);

        response.ShouldBeError();
        response.Description.Should().Contain(reason);
        (await scenario.State.BirthdayAsync(user)).Should().BeNull();
    }

    [Fact]
    public async Task Set_ChangingDayRequiresConfirmationAndPreservesRewardHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        DateTime rewardedAt = new(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);
        await scenario.Given.BirthdayAsync(user, new(year: 2000, month: 1, day: 1), lastRewardAt: rewardedAt);
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set", arguments:
        [
            SlashArgument.Integer("day", value: 13),
            SlashArgument.Integer("month", value: 12),
            SlashArgument.Integer("year", value: 1989),
            SlashArgument.Boolean("privately", value: true),
        ]);
        (await scenario.State.BirthdayAsync(user))!.Birthday.Should().Be(new DateOnly(year: 2000, month: 1, day: 1));

        var response = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        response.ShouldBeSuccess();
        var birthday = await scenario.State.BirthdayAsync(user);
        birthday!.Birthday.Should().Be(new DateOnly(year: 1989, month: 12, day: 13));
        birthday.IsPrivate.Should().BeTrue();
        birthday.LastRewardAt.Should().Be(rewardedAt);
    }

    [Fact]
    public async Task Set_CancelPreservesOriginalBirthday()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user, new(year: 2000, month: 1, day: 1));
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set",
            arguments: [SlashArgument.Integer("day", value: 13), SlashArgument.Integer("month", value: 12)]);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        response.Description.Should().Contain("cancelled");
        (await scenario.State.BirthdayAsync(user))!.Birthday.Should().Be(new DateOnly(year: 2000, month: 1, day: 1));
    }

    [Fact]
    public async Task Set_AnotherUserCannotConfirmBirthdayChange()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user, new(year: 2000, month: 1, day: 1));
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday set",
            arguments: [SlashArgument.Integer("day", value: 13), SlashArgument.Integer("month", value: 12)]);

        var response = await scenario.Discord.ClickAsync(other, prompt, "Confirm");

        response.Requests.Should().ContainSingle("another user's click is acknowledged without editing the original message");
        (await scenario.State.BirthdayAsync(user))!.Birthday.Should().Be(new DateOnly(year: 2000, month: 1, day: 1));
        (await scenario.State.BirthdayAsync(other)).Should().BeNull();
    }

    [Fact]
    public async Task Show_DisplaysSelectedUsersPublicBirthdayWithoutYear()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(other, new(year: 1989, month: 12, day: 13));

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday show", selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("December 13").And.NotContain("1989");
    }

    [Fact]
    public async Task Show_PrivateBirthdayIsNotDisclosed()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(other, new(year: 1989, month: 12, day: 13), isPrivate: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday show", selectedUser: other);

        response.ShouldBeError();
        response.Description.Should().Contain("private").And.NotContain("December 13");
    }

    [Theory]
    [InlineData("birthday show")]
    [InlineData("birthday age")]
    [InlineData("birthday horoscope")]
    public async Task ShowAgeAndHoroscope_UnsetExplainHowToSet(string command)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, command);

        response.ShouldBeError();
        response.Description.Should().Contain("not set").And.Contain("birthday set");
    }

    [Fact]
    public async Task Age_PrivateBirthdayStillAllowsAgeCalculation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Given.BirthdayAsync(other, new(DateTime.UtcNow.Year - 25, month: 1, day: 1), isPrivate: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday age", selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Other").And.Contain("25");
    }

    [Fact]
    public async Task Age_WithoutYearExplainsRequiredOption()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user, new(year: 1804, month: 12, day: 13));

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday age");

        response.ShouldBeError();
        response.Description.Should().Contain("without a year");
    }

    [Fact]
    public async Task Age_AssignsOnlyEligiblePlusServerAgeRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var adult = scenario.Given.Role("Adults");
        var older = scenario.Given.Role("Thirty plus");
        var guild = await scenario.Given.GuildAsync(user, roles: [adult, older]);
        await scenario.Given.PlusAsync(user, maximumGuilds: 2, guild);
        await scenario.Given.AgeRoleAsync(guild, adult, minimumAge: 18);
        await scenario.Given.AgeRoleAsync(guild, older, minimumAge: 30);
        await scenario.Given.BirthdayAsync(user, new(DateTime.UtcNow.Year - 25, month: 1, day: 1));
        scenario.DiscordApi.ExpectRoleChange(guild, user, adult);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday age", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("25");
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Path.Should().EndWith($"/roles/{adult.Id}");
    }

    [Fact]
    public async Task Clear_HidesBirthdayButPreservesRewardHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        DateTime rewardedAt = new(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);
        await scenario.Given.BirthdayAsync(user, new(year: 2000, month: 1, day: 1), lastRewardAt: rewardedAt);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday clear");

        response.ShouldBeSuccess();
        var birthday = await scenario.State.BirthdayAsync(user);
        birthday!.Birthday.Should().Be(DateOnly.MinValue);
        birthday.IsPrivate.Should().BeTrue();
        birthday.LastRewardAt.Should().Be(rewardedAt);
        var show = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday show");
        show.ShouldBeError();
        show.Description.Should().Contain("not set");
    }

    [Fact]
    public async Task LegacyBirthday_DisplaysPublicBirthday()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user, new(year: 1989, month: 12, day: 13));

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!birthday");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("December 13");
    }
}
