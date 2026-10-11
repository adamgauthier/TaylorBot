using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
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

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(50)]
    public async Task InspectCategory_ListsCompleteChannelLinksWithoutNames(int childCount)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var category = await scenario.Given.ChannelAsync(guild, "Category", type: 4);
        List<ScenarioGuild> children = [];
        for (var index = 0; index < childCount; index++)
        {
            children.Add(await scenario.Given.ChannelAsync(guild, index % 2 == 0 ? "Private channel name" : "___hidden___",
                parentId: category.ChannelId, obfuscated: index % 2 != 0));
        }

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", category)]);

        var fields = response.Embed.GetProperty("fields").EnumerateArray()
            .Where(field => field.GetProperty("name").GetString()!.StartsWith("Channels", StringComparison.Ordinal)).ToList();
        fields.Should().ContainSingle();
        fields.Select(field => field.GetProperty("value").GetString()!).Should().OnlyContain(value => value.Length <= 1024);
        if (childCount == 0)
        {
            fields.Single().GetProperty("value").GetString().Should().Be("None");
        }
        else
        {
            var expected = children.Select(child => $"https://discord.com/channels/{guild.Id}/{child.ChannelId}").ToList();
            var links = fields.Single().GetProperty("value").GetString()!.Split(' ');
            if (string.Join(' ', expected).Length <= 1024)
            {
                links.Should().BeEquivalentTo(expected);
            }
            else
            {
                links.Last().Should().Be("...");
                var displayed = links[..^1];
                displayed.Should().NotBeEmpty().And.OnlyContain(link => expected.Contains(link)).And.OnlyHaveUniqueItems();
                displayed.Should().HaveCountLessThan(childCount);
                (string.Join(' ', displayed.Append(expected.Except(displayed).First())).Length + " ...".Length).Should().BeGreaterThan(expected: 1024);
            }
        }
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
