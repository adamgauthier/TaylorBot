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
    public async Task InspectChannel_UsesResolvedMetadataInsteadOfObfuscatedCache()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = await scenario.Given.ChannelAsync(guild, "___hidden___", obfuscated: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Visible to user", Topic: "Visible topic", IncludeTopic: true))]);

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Visible to user");
        response.Field("Topic").Should().Be("Visible topic");
    }

    [Fact]
    public async Task InspectCurrentChannel_UsesInteractionMetadata()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = await scenario.Given.ChannelAsync(guild, "___hidden___", obfuscated: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", channel,
            currentChannel: new(Name: "Current visible channel", Topic: "Current topic", IncludeTopic: true));

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Current visible channel");
        response.Field("Topic").Should().Be("Current topic");
    }

    [Theory]
    [InlineData(false, null, "Cached topic")]
    [InlineData(true, null, "None")]
    [InlineData(true, "", "None")]
    [InlineData(true, "Updated topic", "Updated topic")]
    public async Task InspectChannel_UsesPresentTopicWithoutResurrectingClearedValues(bool includeTopic, string? topic, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = await scenario.Given.ChannelAsync(guild, "Cached channel", topic: "Cached topic");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Topic: topic, IncludeTopic: includeTopic))]);

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Cached channel");
        response.Field("Topic").Should().Be(expected);
    }

    [Fact]
    public async Task InspectChannel_DoesNotRequireCachedSelectedChannel()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = scenario.Given.UncachedChannel(guild);
        var parent = scenario.Given.UncachedChannel(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Resolved only", Topic: "Resolved topic", IncludeTopic: true,
                ParentId: parent.ChannelId, IncludeParent: true))]);

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Resolved only");
        response.Field("Topic").Should().Be("Resolved topic");
        response.Field("Server").Should().Contain(guild.Id);
        response.Field("Category").Should().Contain(parent.ChannelId).And.NotContain("REST channel");
        scenario.DiscordApi.RequestsFor("GET", $"channels/{channel.ChannelId}").Should().ContainSingle();
        scenario.DiscordApi.RequestsFor("GET", $"channels/{parent.ChannelId}").Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InspectChannel_UsesParentIdWithoutDisclosingHiddenCategoryName(bool obfuscatedParent)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var parent = await scenario.Given.ChannelAsync(guild, "Parent category", type: 4, obfuscated: obfuscatedParent);
        var channel = await scenario.Given.ChannelAsync(guild, "Child");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Child", ParentId: parent.ChannelId, IncludeParent: true))]);

        response.Field("Category").Should().Contain(parent.ChannelId);
        response.Field("Category").Contains("Parent category", StringComparison.Ordinal).Should().Be(!obfuscatedParent);
    }

    [Fact]
    public async Task InspectChannel_DoesNotResurrectClearedParent()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var parent = await scenario.Given.ChannelAsync(guild, "Old parent", type: 4);
        var channel = await scenario.Given.ChannelAsync(guild, "Child", parentId: parent.ChannelId);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Child", IncludeParent: true))]);

        response.Embed.GetProperty("fields").EnumerateArray().Select(field => field.GetProperty("name").GetString())
            .Should().NotContain("Category");
    }

    [Fact]
    public async Task InspectVoiceChannel_PreservesCachedVoiceDetails()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = await scenario.Given.ChannelAsync(guild, "___hidden___", type: 2, obfuscated: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Voice visible to user", Type: 2))]);

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Voice visible to user");
        response.Field("Type").Should().Be("Voice");
        response.Field("Bitrate").Should().Contain("64000");
        response.Field("User Limit").Should().Be("7");
    }

    [Fact]
    public async Task InspectChannel_UsesCachedParentWhenInteractionOmitsIt()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var parent = await scenario.Given.ChannelAsync(guild, "Cached parent", type: 4);
        var channel = await scenario.Given.ChannelAsync(guild, "Child", parentId: parent.ChannelId);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Child"))]);

        response.Field("Category").Should().Contain("Cached parent").And.Contain(parent.ChannelId);
    }

    [Fact]
    public async Task InspectChannel_UsesInteractionNsfwFlagInsteadOfStaleCache()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var channel = await scenario.Given.ChannelAsync(guild, "Old name", nsfw: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel", guild,
            arguments: [SlashArgument.Channel("channel", channel, new(Name: "Updated channel", Nsfw: false))]);

        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("Updated channel");
    }

    [Fact]
    public async Task InspectDmChannel_PreservesLookupWithoutInteractionMetadata()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.DiscordApi.ExpectDmChannel(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "inspect channel");

        response.Field("Type").Should().Be("DM");
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
            arguments: [SlashArgument.Channel("channel", category, new(Name: "Category", Type: 4))]);

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
