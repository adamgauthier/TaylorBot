using FluentAssertions;
using System.Text.Json.Nodes;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "owner ignore")]
[Trait("Command", "owner reward")]
[Trait("Command", "owner diagnostic")]
[Trait("Command", "owner addfeedbackusers")]
[Trait("Command", "owner downloadavatars")]
[Trait("Command", "owner rewardyearbook")]
public sealed class OwnerTests(DataServices data)
{
    [Fact]
    public async Task Ignore_PersistsRequestedDuration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync();
        var before = DateTimeOffset.UtcNow;

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner ignore",
            selectedUser: user, arguments: [SlashArgument.Text("time", "2h")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(user.Id).And.Contain("2 hours");
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeOnOrAfter(before.AddHours(2)).And.BeBefore(DateTimeOffset.UtcNow.AddHours(2));
    }

    [Fact]
    public async Task Ignore_RejectsSelfWithoutChangingIgnoreState()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner ignore",
            selectedUser: owner, arguments: [SlashArgument.Text("time", "2h")]);

        response.ShouldBeError();
        response.Description.Should().Contain("yourself");
        (await scenario.State.IgnoredUntilAsync(owner)).Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Ignore_InvalidDurationDoesNotIgnoreRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner ignore",
            selectedUser: user, arguments: [SlashArgument.Text("time", "not-a-duration")]);

        response.ShouldBeError();
        response.Description.Should().Contain("time");
        (await scenario.State.IgnoredUntilAsync(user)).Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Reward_CreditsEachSelectedUserAndReportsBalances()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var first = await scenario.Given.UserAsync(taypoints: 9);
        var second = await scenario.Given.UserAsync(taypoints: 31);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward", arguments:
            [SlashArgument.Integer("amount", value: 13), SlashArgument.User("user1", first), SlashArgument.User("user2", second)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(first.Id).And.Contain(second.Id).And.Contain("22").And.Contain("44");
        (await scenario.State.BalanceAsync(first)).Should().Be(22);
        (await scenario.State.BalanceAsync(second)).Should().Be(44);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Reward_RejectsNonPositiveAmount(long amount)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner reward",
            arguments: [SlashArgument.Integer("amount", amount), SlashArgument.User("user1", user)]);

        response.ShouldBeError();
        (await scenario.State.BalanceAsync(user)).Should().Be(9);
    }

    [Fact]
    public async Task Reward_RejectsNonOwnerWithoutCreditingRecipient()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(taypoints: 9);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "owner reward",
            arguments: [SlashArgument.Integer("amount", value: 13), SlashArgument.User("user1", recipient)]);

        response.ShouldBeError();
        response.Description.Should().Contain("owner");
        (await scenario.State.BalanceAsync(recipient)).Should().Be(9);
    }

    [Fact]
    public async Task Diagnostic_ReportsLiveGatewayCacheAndShardCount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var guild = await scenario.Given.GuildAsync(owner);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner diagnostic", guild);

        response.ShouldBeSuccess();
        response.Field("Guild Cache").Should().Be("1");
        response.Field("Shard Count").Should().Be("1");
        response.Field("DM Channels Cache").Should().Be("0");
        response.Field("Latency").Should().EndWith(" ms");
    }

    [Fact]
    public async Task AddFeedbackUsers_AddsRoleOnlyToEligibleMembers()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync();
        ScenarioRole role = new("482738450722848809", "Feedback");
        var guild = await scenario.Given.GuildAsync(owner, roles: [role], id: "115332333745340416");
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.FeedbackEligibleAsync(guild, user);
        scenario.DiscordApi.ExpectRoleChange(guild, user, role);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner addfeedbackusers", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Added **1** members").And.Contain("total of **1**");
        scenario.DiscordApi.RoleChanges.Should().ContainSingle().Which.Path.Should().Contain(user.Id);
    }

    [Fact]
    public async Task AddFeedbackUsers_SimulationDoesNotAssignRoles()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner, id: "115332333745340416");
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.FeedbackEligibleAsync(guild, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner addfeedbackusers", guild,
            arguments: [new("whatif", Type: 5, JsonValue.Create(value: true))]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("[SIMULATION]").And.Contain("Added **1**");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task AddFeedbackUsers_DoesNotReassignExistingFeedbackRole()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync();
        ScenarioRole role = new("482738450722848809", "Feedback");
        var guild = await scenario.Given.GuildAsync(owner, roles: [role], id: "115332333745340416");
        await scenario.Given.MemberAsync(guild, user, role);
        await scenario.Given.FeedbackEligibleAsync(guild, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner addfeedbackusers", guild);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Added **0**").And.Contain("**1** members already had");
        scenario.DiscordApi.RoleChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadAvatars_CopiesDownloadedBytesToNamedAzureBlob()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync(username: "AvatarUser", avatar: "synthetic-avatar");
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, user);
        var image = scenario.External.AvatarTransfer(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner downloadavatars", guild,
            arguments: [SlashArgument.Text("userids", user.Id)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Downloaded **1** avatars").And.Contain("**0** members");
        scenario.External.Requests.Should().ContainSingle(request => request.Method == "PUT")
            .Which.Bytes.Should().Equal(image);
    }

    [Fact]
    public async Task DownloadAvatars_DirectMessageCannotDownloadGuildMembers()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner downloadavatars",
            arguments: [SlashArgument.Text("userids", owner.Id)]);

        response.ShouldBeError();
        response.Description.Should().Contain("server");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false, 10000, false)]
    [InlineData(true, 25000, false)]
    [InlineData(false, 10000, true)]
    public async Task RewardYearbook_CreditsMemberNotifiesAndPersistsCompletion(bool isMod, long reward, bool signatureSubmitted)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.YearbookAsync(user, isMod);
        scenario.External.SignatureList(user, exists: signatureSubmitted);
        ModerationDiscordFixtures.ExpectDirectMessage(scenario.DiscordApi, user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner rewardyearbook", guild,
            arguments: [SlashArgument.Integer("count", value: 1)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Messaged **1**");
        (await scenario.State.BalanceAsync(user)).Should().Be(13 + reward);
        var progress = (await scenario.State.YearbookAsync()).GetProperty("members")[0].GetProperty("processedInfo");
        progress.GetProperty("completed").GetBoolean().Should().BeTrue();
        progress.GetProperty("rewarded").GetBoolean().Should().BeTrue();
        progress.GetProperty("messaged").GetBoolean().Should().BeTrue();
        var message = scenario.DiscordApi.RequestsFor("POST", $"channels/{ModerationDiscordFixtures.DirectMessageChannelId}/messages")
            .Should().ContainSingle().Which.Body!.Value;
        message.GetProperty("embeds")[0].GetProperty("description").GetString()!
            .Contains("Signature MISSING", StringComparison.Ordinal).Should().Be(!signatureSubmitted);
    }

    [Fact]
    public async Task RewardYearbook_SkipsCompletedMembersWithoutDuplicatePayment()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.YearbookAsync(user, completed: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner rewardyearbook", guild,
            arguments: [SlashArgument.Integer("count", value: 1)]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Messaged **0**");
        (await scenario.State.BalanceAsync(user)).Should().Be(13);
        scenario.External.Requests.Should().BeEmpty();
        scenario.DiscordApi.RequestsFor("POST", "users/@me/channels").Should().BeEmpty();
    }

    [Fact]
    public async Task RewardYearbook_RejectsZeroBatchSizeWithoutRewarding()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync(botOwner: true);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        var guild = await scenario.Given.GuildAsync(owner);
        await scenario.Given.YearbookAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(owner, "owner rewardyearbook", guild,
            arguments: [SlashArgument.Integer("count", value: 0)]);

        response.ShouldBeError();
        (await scenario.State.BalanceAsync(user)).Should().Be(13);
        (await scenario.State.YearbookAsync()).GetProperty("members")[0].GetProperty("processedInfo")
            .GetProperty("completed").GetBoolean().Should().BeFalse();
        scenario.External.Requests.Should().BeEmpty();
    }
}
