using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.DiscordInfo;

[Trait("Command", "inspect user")]
[Trait("Command", "inspect role")]
[Trait("Command", "inspect channel")]
[Trait("Command", "inspect server")]
public sealed class DiscordInfoTests(DataServices data)
{
    [Fact]
    public async Task InspectUser_ShowsSelectedUsersId()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect user", selectedUser: other);

        response.Field("User Id").Should().Contain(other.Id);
    }

    [Fact]
    public async Task InspectChannel_ShowsResolvedChannelId()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", guild)]);

        response.Field("Id").Should().Contain(guild.ChannelId);
    }

    [Fact]
    public async Task InspectRole_ShowsResolvedRoleId()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var role = scenario.Given.Role();
        var guild = await scenario.Given.GuildAsync(user, roles: [role]);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect role", guild,
            arguments: [SlashArgument.Role("role", role)]);

        response.Field("Id").Should().Contain(role.Id);
    }

    [Fact]
    public async Task InspectServer_ShowsGatewayCachedGuildId()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect server", guild);

        response.Field("Id").Should().Contain(guild.Id);
    }

    [Fact]
    public async Task LegacyAvatar_UsesTheSelectedUsersAvatar()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(avatar: "synthetic-avatar");
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!avatar <@{user.Id}>");

        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().StartWith($"https://cdn.discordapp.com/avatars/{user.Id}/synthetic-avatar");
    }
}
