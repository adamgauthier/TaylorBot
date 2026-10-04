using FluentAssertions;
using System.Net;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Moderation;

[Trait("Command", "modmail config")]
[Trait("Command", "modmail block")]
[Trait("Command", "modmail unblock")]
[Trait("Command", "modmail message-mods")]
[Trait("Command", "modmail message-user")]
public sealed class ModmailTests(DataServices data)
{
    [Fact]
    public async Task Block_FreeServerAtLimitDoesNotBlockAnotherUser()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.ModmailBlockLimitAsync(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail block", guild, selectedUser: user);

        response.ShouldBeError();
        response.Description.Should().Contain("limit").And.Contain("50");
        (await scenario.State.ModmailBlockedAsync(guild, user)).Should().BeFalse();
    }

    [Fact]
    public async Task Block_PlusServerCanExceedFreeLimit()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.PlusAsync(moderator, activeGuilds: [guild]);
        await scenario.Given.ModmailBlockLimitAsync(guild);
        await scenario.Given.LogChannelAsync(guild, "modmail");
        scenario.DiscordApi.ExpectModerationLog(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail block", guild, selectedUser: user);

        response.ShouldBeSuccess();
        (await scenario.State.ModmailBlockedAsync(guild, user)).Should().BeTrue();
        scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value.GetProperty("embeds")[0]
            .GetProperty("footer").GetProperty("text").GetString().Should().Be("User blocked from sending mod mail");
    }

    [Theory]
    [InlineData("modmail block")]
    [InlineData("modmail unblock")]
    public async Task BlockCommands_RejectSelf(string route)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);

        var response = await scenario.Discord.InvokeSlashCommandAsync(moderator, route, guild, selectedUser: moderator);

        response.ShouldBeError();
        response.Description.Should().Contain("yourself");
        (await scenario.State.ModmailBlockedAsync(guild, moderator)).Should().BeFalse();
    }

    [Fact]
    public async Task Config_ChannelSelectEnablesModmailAndStopDisablesIt()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);

        var config = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        var enabled = await scenario.Discord.SelectChannelAsync(moderator, config, guild);

        enabled.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().Be(guild.ChannelId);

        var updated = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        var disabled = await scenario.Discord.ClickAsync(moderator, updated, "Disable Mod Mail");

        updated.Description.Should().Contain(guild.ChannelId);
        disabled.Description.Should().Contain("disabled");
        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().BeNull();
    }

    [Fact]
    public async Task Config_ChangingChannelRequiresConfirmation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.LogChannelAsync(guild, "modmail", "100000000000000099");

        var config = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        var prompt = await scenario.Discord.SelectChannelAsync(moderator, config, guild);

        config.Description.Should().Contain("can't find");
        prompt.Description.Should().Contain("NOT");
        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().Be("100000000000000099");

        var confirmed = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        confirmed.ShouldBeSuccess();
        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().Be(guild.ChannelId);
    }

    [Fact]
    public async Task Config_CancelChannelChangePreservesOriginalConfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.LogChannelAsync(guild, "modmail", "100000000000000099");

        var config = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail config", guild);
        var prompt = await scenario.Discord.SelectChannelAsync(moderator, config, guild);
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Cancel");

        response.Description.Should().ContainEquivalentOf("cancel");
        (await scenario.State.LogChannelAsync(guild, "modmail")).Should().Be("100000000000000099");
    }

    [Fact]
    public async Task BlockAndUnblock_PersistAndExplainMissingLogConfiguration()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);

        var blocked = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail block", guild, selectedUser: user);

        blocked.Description.Should().Contain("Blocked").And.Contain(user.Id);
        (await scenario.State.ModmailBlockedAsync(guild, user)).Should().BeTrue();

        var unblocked = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail unblock", guild, selectedUser: user);

        unblocked.Description.Should().Contain("Unblocked").And.Contain(user.Id);
        (await scenario.State.ModmailBlockedAsync(guild, user)).Should().BeFalse();
    }

    [Fact]
    public async Task MessageMods_ModalAndConfirmationDeliverSubjectAndMessageToLog()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "modmail");
        scenario.DiscordApi.ExpectModerationLog(guild);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        var prompt = await scenario.Discord.SubmitModalAsync(user, modal, new Dictionary<string, string>
        {
            ["subject"] = "A moderation question",
            ["messagecontent"] = "Please clarify the spoiler rule.",
        });

        modal.Modal.GetProperty("title").GetString().Should().Be("Send Message to Moderators");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();

        var sent = await scenario.Discord.ClickAsync(user, prompt, "Confirm");
        var delivered = scenario.DiscordApi.ModerationLogs(guild).Should().ContainSingle().Which.Body!.Value;

        sent.ShouldBeSuccess();
        delivered.GetProperty("embeds")[0].GetProperty("title").GetString().Should().Be("A moderation question");
        delivered.GetProperty("embeds")[0].GetProperty("description").GetString().Should().Be("Please clarify the spoiler rule.");
        delivered.GetProperty("components")[0].GetProperty("components")[0].GetProperty("label").GetString().Should().Be("Reply");
    }

    [Fact]
    public async Task MessageMods_ReplyButtonDeliversResponseAndLogsOriginalMessageReference()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var moderator = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, user);
        await scenario.Given.LogChannelAsync(guild, "modmail");
        scenario.DiscordApi.ExpectModerationLog(guild);

        var compose = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        var prompt = await scenario.Discord.SubmitModalAsync(user, compose, new Dictionary<string, string>
        {
            ["subject"] = "Rules",
            ["messagecontent"] = "Can I post this?",
        });
        await scenario.Discord.ClickAsync(user, prompt, "Confirm");
        DiscordExchange received = new(scenario.DiscordApi.ModerationLogs(guild), moderator, guild);
        var reply = await scenario.Discord.ClickAsync(moderator, received, "Reply");
        var replyPrompt = await scenario.Discord.SubmitModalAsync(moderator, reply, new Dictionary<string, string>
        {
            ["messagecontent"] = "Yes, in the relevant channel.",
        });

        reply.Modal.GetProperty("title").GetString().Should().Be("Send Mod Mail to User");
        replyPrompt.Message.GetProperty("embeds")[0].GetProperty("description").GetString().Should().Be("Yes, in the relevant channel.");
        replyPrompt.Message.GetProperty("embeds")[1].GetProperty("description").GetString().Should().Contain(user.Id);

        ModerationDiscordFixtures.ExpectDirectMessage(scenario.DiscordApi, user);
        ModerationDiscordFixtures.ExpectChannelMessage(scenario.DiscordApi, guild.ChannelId);
        var sent = await scenario.Discord.ClickAsync(moderator, replyPrompt, "Confirm");
        var logged = scenario.DiscordApi.RequestsFor("POST", $"channels/{guild.ChannelId}/messages")[^1].Body!.Value;

        sent.ShouldBeSuccess();
        logged.GetProperty("embeds")[0].GetProperty("description").GetString().Should().Be("Yes, in the relevant channel.");
        logged.GetProperty("embeds")[0].GetProperty("footer").GetProperty("text").GetString().Should().Be("Mod mail sent");
        logged.GetProperty("message_reference").GetProperty("message_id").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task MessageMods_BlockedSenderCannotConfirmDelivery()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "modmail");
        await scenario.Given.BlockedModmailAsync(guild, user);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        var prompt = await scenario.Discord.SubmitModalAsync(user, modal, new Dictionary<string, string>
        {
            ["subject"] = "",
            ["messagecontent"] = "Please let me send this.",
        });
        var rejected = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        prompt.Message.GetProperty("embeds")[0].GetProperty("title").GetString().Should().Be("No Subject");
        rejected.ShouldBeError();
        rejected.Description.Should().Contain("blocked");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task MessageMods_CancelDoesNotDeliverDraftToModerators()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "modmail");

        var modal = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        var prompt = await scenario.Discord.SubmitModalAsync(user, modal, new Dictionary<string, string>
        {
            ["subject"] = "Draft",
            ["messagecontent"] = "Do not send this.",
        });
        var cancelled = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        cancelled.Description.Should().ContainEquivalentOf("cancel");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task MessageMods_UnconfiguredServerDoesNotOfferDelivery()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(user, "modmail message-mods", guild);
        var response = await scenario.Discord.SubmitModalAsync(user, modal, new Dictionary<string, string>
        {
            ["subject"] = "",
            ["messagecontent"] = "Hello moderators.",
        });

        response.Description.Should().Contain("hasn't enabled");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task MessageUser_ConfirmedMessageIsDeliveredPrivately()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, user);
        ModerationDiscordFixtures.ExpectDirectMessage(scenario.DiscordApi, user);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail message-user", guild, selectedUser: user);
        var prompt = await scenario.Discord.SubmitModalAsync(moderator, modal, new Dictionary<string, string>
        {
            ["messagecontent"] = "Thanks for reporting this.",
        });
        var sent = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        sent.Description.Should().Contain("Message sent").And.Contain(user.Id);
        var message = scenario.DiscordApi.RequestsFor("POST", $"channels/{ModerationDiscordFixtures.DirectMessageChannelId}/messages").Should().ContainSingle().Which.Body!.Value;
        message.GetProperty("embeds")[0].GetProperty("description").GetString().Should().Be("Thanks for reporting this.");
        message.GetProperty("embeds")[0].GetProperty("title").GetString().Should().Be("Message from the moderation team");
    }

    [Fact]
    public async Task MessageUser_ClosedDirectMessagesExplainPrivacySettings()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, user);
        ModerationDiscordFixtures.ExpectClosedDirectMessages(scenario.DiscordApi, user);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail message-user", guild, selectedUser: user);
        var prompt = await scenario.Discord.SubmitModalAsync(moderator, modal, new Dictionary<string, string>
        {
            ["messagecontent"] = "Please review the rules.",
        });
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Confirm");

        response.ShouldBeError();
        response.Description.Should().Contain("DM settings");
        scenario.DiscordApi.ModerationLogs(guild).Should().BeEmpty();
    }

    [Fact]
    public async Task MessageUser_CancelDoesNotSendDirectMessage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var moderator = await scenario.Given.UserAsync();
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(moderator);
        await scenario.Given.MemberAsync(guild, user);

        var modal = await scenario.Discord.InvokeSlashCommandAsync(moderator, "modmail message-user", guild, selectedUser: user);
        var prompt = await scenario.Discord.SubmitModalAsync(moderator, modal, new Dictionary<string, string>
        {
            ["messagecontent"] = "Discard this draft.",
        });
        var response = await scenario.Discord.ClickAsync(moderator, prompt, "Cancel");

        response.Description.Should().ContainEquivalentOf("cancel");
        scenario.DiscordApi.RequestsFor("POST", "users/@me/channels").Should().BeEmpty();
    }
}

internal static class ModerationDiscordFixtures
{
    internal const string DirectMessageChannelId = "100000000000000088";

    internal static void ExpectDirectMessage(DiscordApi api, ScenarioUser user)
    {
        ExpectDirectMessageChannel(api, user);
        ExpectChannelMessage(api, DirectMessageChannelId);
    }

    internal static void ExpectChannelMessage(DiscordApi api, string channelId)
    {
        api.ExpectRequest("POST", $"channels/{channelId}/messages", new
        {
            id = "100000000000000089",
            channel_id = channelId,
            type = 0,
            content = "",
            author = DiscordDriver.UserPayload(new(DiscordApi.ApplicationId, "IntegrationBot")),
            timestamp = "2026-01-01T00:00:00Z",
            attachments = Array.Empty<object>(),
            embeds = Array.Empty<object>(),
        }, HttpStatusCode.OK);
    }

    internal static void ExpectClosedDirectMessages(DiscordApi api, ScenarioUser user)
    {
        ExpectDirectMessageChannel(api, user);
        api.ExpectRequest("POST", $"channels/{DirectMessageChannelId}/messages", new { code = 50007, message = "Cannot send messages to this user" }, HttpStatusCode.Forbidden);
    }

    private static void ExpectDirectMessageChannel(DiscordApi api, ScenarioUser user) =>
        api.ExpectRequest("POST", "users/@me/channels", new
        {
            id = DirectMessageChannelId,
            type = 1,
            recipients = new[] { DiscordDriver.UserPayload(user) },
        }, HttpStatusCode.OK);
}
