using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Gender;

[Trait("Command", "gender show")]
[Trait("Command", "gender set")]
[Trait("Command", "gender clear")]
public sealed class GenderTests(DataServices data)
{
    [Fact]
    public async Task Set_UpdatesExistingGender()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.TextAttributeAsync(user, "gender", "Female");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "gender set", arguments: [SlashArgument.Text("gender", "Other")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Other");
        (await scenario.State.ProfileTextAsync(user, "gender")).Should().Be("Other");
    }

    [Fact]
    public async Task Show_UsesSelectedUsersGender()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Given.TextAttributeAsync(other, "gender", "Female");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "gender show", selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(other.Id).And.Contain("Female");
    }

    [Fact]
    public async Task Show_UnsetExplainsHowToSet()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "gender show");

        response.ShouldBeError();
        response.Description.Should().Contain("not set").And.Contain("gender set");
    }

    [Fact]
    public async Task Clear_RemovesOnlyCallersGender()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.TextAttributeAsync(user, "gender", "Other");
        await scenario.Given.TextAttributeAsync(other, "gender", "Female");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "gender clear");

        response.ShouldBeSuccess();
        (await scenario.State.ProfileTextAsync(user, "gender")).Should().BeNull();
        (await scenario.State.ProfileTextAsync(other, "gender")).Should().Be("Female");
    }

    [Fact]
    public async Task LegacyShow_DisplaysStoredGender()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.TextAttributeAsync(user, "gender", "Other");

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!gender");

        response.ShouldBeSuccess();
        response.Description.Should().Contain(user.Id).And.Contain("Other");
    }
}
