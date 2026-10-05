using System.Net;
using System.Text.Json;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class BirthdayRoleNotificationTests(DataServices data)
{
    [Fact]
    public async Task RejectedAssignment_NotifiesModLogWithoutRecordingSuccess()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user);
        var role = await scenario.Given.BirthdayRoleAsync(guild);
        await scenario.Given.ModLogAsync(guild);
        scenario.DiscordApi.Expect("PUT", $"guilds/{guild.Id}/members/{user.Id}/roles/{role}",
            new { code = 50013, message = "Missing Permissions" }, HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<BirthdayRoleDomainService>("Exception occurred when adding birthday role");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Text.Should().Contain($"<@&{role}>").And.Contain("assign").And.Contain("Manage Roles");
        AssertMentionsDisabled(output.Messages.Single().Body!.Value);
        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
    }

    [Theory]
    [InlineData(UserNotifierJob.BirthdayRoleAdd, 50001)]
    [InlineData(UserNotifierJob.BirthdayRoleAdd, 10011)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove, 50013)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove, 50001)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove, 10011)]
    public async Task ActionableFailure_NotifiesWithoutCompletingRoleWork(UserNotifierJob job, int discordCode)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: job);
        var command = scenario.DiscordApi.GlobalCommand("birthday");
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, job);
        await scenario.Given.ModLogAsync(guild);
        ExpectFailure(scenario, guild, user, role, job, discordCode);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().ContainSingle();
        output.Text.Should().Contain(job == UserNotifierJob.BirthdayRoleAdd ? "assign" : "remove");
        output.Text.Should().Contain(discordCode == 10011 ? "/birthday role" : "Manage Roles");
        if (discordCode == 10011)
        {
            output.Text.Should().Contain($"</birthday role:{command}>").And.Contain("no longer exists").And.NotContain(role).And.NotContain("<@");
        }
        else
        {
            output.Text.Should().Contain($"<@&{role}>").And.Contain("Server Settings > Roles");
        }
        AssertMentionsDisabled(output.Messages.Single().Body!.Value);
        output.Text.Should().NotContain("@everyone").And.NotContain("@here");
        output.Text.Should().NotContain("sensitive diagnostic");
        if (job == UserNotifierJob.BirthdayRoleAdd)
        {
            (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
        }
        else
        {
            (await scenario.State.BirthdayRoleRemovedAsync(guild, user)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task MissingConfiguredRole_NotifiesOnceAndContinuesOtherMembers()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var command = scenario.DiscordApi.GlobalCommand("birthday");
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Discord.JoinAsync(guild, other);
        await scenario.Given.BirthdayAsync(user);
        await scenario.Given.BirthdayAsync(other);
        var role = await scenario.Given.BirthdayRoleAsync(guild, exists: false);
        await scenario.Given.ModLogAsync(guild);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Text.Should().Contain("no longer exists").And.Contain($"</birthday role:{command}>").And.NotContain(role).And.NotContain("<@");
        output.Messages.Should().ContainSingle();
        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
        (await scenario.State.BirthdayRoleRecordedAsync(guild, other)).Should().BeFalse();
    }

    [Theory]
    [InlineData(UserNotifierJob.BirthdayRoleAdd, false)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove, false)]
    [InlineData(UserNotifierJob.BirthdayRoleAdd, true)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove, true)]
    public async Task UnconfiguredLogOrUnexpectedError_OnlyLogsInternally(UserNotifierJob job, bool unexpectedError)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, job);
        if (unexpectedError)
        {
            await scenario.Given.ModLogAsync(guild);
        }
        ExpectFailure(scenario, guild, user, role, job, discordCode: unexpectedError ? 0 : 50013);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().BeEmpty();
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, job)).Should().BeNull();
    }

    [Fact]
    public async Task MissingRolesInDifferentServers_ReuseCommandLookup()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var command = scenario.DiscordApi.GlobalCommand("birthday");
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user);
        var guild = await scenario.Given.GuildAsync(user);
        var otherGuild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayRoleAsync(guild, exists: false);
        await scenario.Given.BirthdayRoleAsync(otherGuild, exists: false);
        await scenario.Given.ModLogAsync(guild);
        await scenario.Given.ModLogAsync(otherGuild);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(otherGuild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().HaveCount(2);
        output.Text.Should().Contain($"</birthday role:{command}>", Exactly.Twice());
        output.Requests.Should().ContainSingle(request => request.Method == "GET" && request.Path.EndsWith("/commands", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UndeployedBirthdayCommand_KeepsReadableGuidance()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        scenario.DiscordApi.GlobalCommand("help");
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.BirthdayAsync(user);
        await scenario.Given.BirthdayRoleAsync(guild, exists: false);
        await scenario.Given.ModLogAsync(guild);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().ContainSingle();
        output.Text.Should().Contain("/birthday role").And.NotContain("</birthday role:");
    }

    [Fact]
    public async Task MissingRemovalRole_CompletesCleanupWithoutAlert()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleRemove);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await scenario.Given.BirthdayRoleAsync(guild, exists: false);
        await scenario.Given.BirthdayRoleGivenAsync(guild, user, role, hasRole: false);
        await scenario.Given.ModLogAsync(guild);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleRemove);

        output.Messages.Should().BeEmpty();
        (await scenario.State.BirthdayRoleRemovedAsync(guild, user)).Should().BeTrue();
    }

    [Theory]
    [InlineData(UserNotifierJob.BirthdayRoleAdd)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove)]
    public async Task RepeatedFailures_NotifyAgainOnlyAfterRedisExpiry(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, job);
        await scenario.Given.ModLogAsync(guild);
        ExpectFailure(scenario, guild, user, role, job);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        await scenario.RunJobAsync(job);
        var cooldown = await scenario.State.BirthdayRoleAlertExpiryAsync(guild, job);
        ExpectFailure(scenario, guild, user, role, job);

        var suppressed = await scenario.RunJobAsync(job);
        await scenario.Given.ExpireBirthdayRoleAlertAsync(guild, job);
        ExpectFailure(scenario, guild, user, role, job);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        var repeated = await scenario.RunJobAsync(job);

        cooldown.Should().NotBeNull();
        cooldown!.Value.Should().BeCloseTo(TimeSpan.FromHours(24), precision: TimeSpan.FromSeconds(30));
        suppressed.Messages.Should().BeEmpty();
        repeated.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task MultipleMembers_ShareOneWarningAndOtherServersContinue()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Discord.JoinAsync(guild, other);
        var role = await ArrangeRoleAsync(scenario, guild, user, UserNotifierJob.BirthdayRoleAdd);
        await scenario.Given.BirthdayAsync(other);
        await scenario.Given.ModLogAsync(guild);
        var otherGuild = await scenario.Given.GuildAsync(user);
        var otherRole = await scenario.Given.BirthdayRoleAsync(otherGuild);
        await scenario.Given.ModLogAsync(otherGuild);
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);
        ExpectFailure(scenario, guild, other, role, UserNotifierJob.BirthdayRoleAdd);
        ExpectFailure(scenario, otherGuild, user, otherRole, UserNotifierJob.BirthdayRoleAdd);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(otherGuild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().HaveCount(2);
        output.Text.Should().Contain(role).And.Contain(otherRole);
    }

    [Fact]
    public async Task RejectedWarning_UsesShortBackoffThenRetriesNewDestination()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, UserNotifierJob.BirthdayRoleAdd);
        await scenario.Given.ModLogAsync(guild);
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);
        scenario.DiscordApi.RejectMessage(guild.ChannelId, code: 50013);
        await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);
        var retryExpiry = await scenario.State.BirthdayRoleAlertExpiryAsync(guild, UserNotifierJob.BirthdayRoleAdd);
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);

        var suppressed = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);
        var channel = await scenario.Discord.CreateChannelAsync(guild, "fixed-mod-log");
        await scenario.Given.ModLogAsync(guild, channel);
        await scenario.Given.ExpireBirthdayRoleAlertAsync(guild, UserNotifierJob.BirthdayRoleAdd);
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);
        scenario.DiscordApi.ExpectMessage(channel);
        var delivered = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        retryExpiry.Should().NotBeNull();
        retryExpiry!.Value.Should().BeCloseTo(TimeSpan.FromMinutes(5), precision: TimeSpan.FromSeconds(30));
        suppressed.Messages.Should().BeEmpty();
        delivered.Messages.Single().Path.Should().Be($"channels/{channel}/messages");
        scenario.Logs.ToString().Should().Contain("Could not notify guild");
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, UserNotifierJob.BirthdayRoleAdd))!.Value
            .Should().BeCloseTo(TimeSpan.FromHours(24), precision: TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task AssignmentAndRemoval_HaveIndependentCooldowns()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken,
            job: UserNotifierJob.BirthdayRoleAdd,
            settings: new Dictionary<string, string?> { ["UserNotifierStartup:BirthdayRoleRemoveInitialDelay"] = "00:00:01" });
        var birthday = await scenario.Given.UserAsync();
        var expired = await scenario.Given.UserAsync(username: "Expired");
        var guild = await scenario.Given.GuildAsync(birthday);
        await scenario.Discord.JoinAsync(guild, expired);
        var role = await ArrangeRoleAsync(scenario, guild, birthday, UserNotifierJob.BirthdayRoleAdd);
        await scenario.Given.BirthdayRoleGivenAsync(guild, expired, role);
        await scenario.Given.ModLogAsync(guild);
        ExpectFailure(scenario, guild, birthday, role, UserNotifierJob.BirthdayRoleAdd);
        ExpectFailure(scenario, guild, expired, role, UserNotifierJob.BirthdayRoleRemove);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().HaveCount(2);
        output.Text.Should().Contain("assign").And.Contain("remove");
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, UserNotifierJob.BirthdayRoleAdd)).Should().NotBeNull();
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, UserNotifierJob.BirthdayRoleRemove)).Should().NotBeNull();
    }

    [Fact]
    public async Task DeletedModLog_LogsDeliveryProblemAndRetainsRoleRetry()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, UserNotifierJob.BirthdayRoleAdd);
        await scenario.Given.ModLogAsync(guild, channelId: "100000000000099999");
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().BeEmpty();
        scenario.Logs.ToString().Should().Contain("is unavailable in guild");
        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, UserNotifierJob.BirthdayRoleAdd))!.Value
            .Should().BeCloseTo(TimeSpan.FromMinutes(5), precision: TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData(UserNotifierJob.BirthdayRoleAdd)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove)]
    public async Task DepartedMember_DoesNotNotifyModerators(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await ArrangeRoleAsync(scenario, guild, user, job);
        await scenario.Given.ModLogAsync(guild);
        await scenario.Discord.LeaveAsync(guild, user);
        scenario.DiscordApi.Resource($"guilds/{guild.Id}/members/{user.Id}",
            new { code = 10007, message = "Unknown Member" }, HttpStatusCode.NotFound);

        var output = await scenario.RunJobAsync(job);

        output.Messages.Should().BeEmpty();
        (await scenario.State.BirthdayRoleAlertExpiryAsync(guild, job)).Should().BeNull();
    }

    [Fact]
    public async Task UnavailableModLogConfiguration_LogsInternallyWithoutSendingAlert()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: UserNotifierJob.BirthdayRoleAdd);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, UserNotifierJob.BirthdayRoleAdd);
        await scenario.Given.ModLogAsync(guild);
        await using var failure = await scenario.Given.UnavailableModLogConfigurationAsync();
        ExpectFailure(scenario, guild, user, role, UserNotifierJob.BirthdayRoleAdd);

        var output = await scenario.RunJobAsync(UserNotifierJob.BirthdayRoleAdd);

        output.Messages.Should().BeEmpty();
        scenario.Logs.ToString().Should().Contain("Could not notify guild");
        (await scenario.State.BirthdayRoleRecordedAsync(guild, user)).Should().BeFalse();
    }

    [Theory]
    [InlineData(UserNotifierJob.BirthdayRoleAdd)]
    [InlineData(UserNotifierJob.BirthdayRoleRemove)]
    public async Task FixedPermissions_ResumeRoleWorkDuringNotificationCooldown(UserNotifierJob job)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken, job: job);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        var role = await ArrangeRoleAsync(scenario, guild, user, job);
        await scenario.Given.ModLogAsync(guild);
        ExpectFailure(scenario, guild, user, role, job);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        await scenario.RunJobAsync(job);
        scenario.DiscordApi.Expect(job == UserNotifierJob.BirthdayRoleAdd ? "PUT" : "DELETE",
            $"guilds/{guild.Id}/members/{user.Id}/roles/{role}");

        var recovered = await scenario.RunJobAsync(job);

        recovered.Messages.Should().BeEmpty();
        var completed = job == UserNotifierJob.BirthdayRoleAdd
            ? await scenario.State.BirthdayRoleRecordedAsync(guild, user)
            : await scenario.State.BirthdayRoleRemovedAsync(guild, user);
        completed.Should().BeTrue();
    }

    private static void AssertMentionsDisabled(JsonElement body)
    {
        var allowedMentions = body.GetProperty("allowed_mentions");
        allowedMentions.ValueKind.Should().Be(JsonValueKind.Object);
        foreach (var field in new[] { "parse", "roles", "users" })
        {
            if (allowedMentions.TryGetProperty(field, out var mentions) && mentions.ValueKind != JsonValueKind.Null)
            {
                mentions.EnumerateArray().Should().BeEmpty();
            }
        }
    }

    private static async Task<string> ArrangeRoleAsync(UserNotifierScenario scenario, ScenarioGuild guild, ScenarioUser user, UserNotifierJob job)
    {
        var role = await scenario.Given.BirthdayRoleAsync(guild);
        if (job == UserNotifierJob.BirthdayRoleAdd)
        {
            await scenario.Given.BirthdayAsync(user);
        }
        else
        {
            await scenario.Given.BirthdayRoleGivenAsync(guild, user, role);
        }

        return role;
    }

    private static void ExpectFailure(UserNotifierScenario scenario, ScenarioGuild guild, ScenarioUser user, string role,
        UserNotifierJob job, int discordCode = 50013)
    {
        var adding = job == UserNotifierJob.BirthdayRoleAdd;
        scenario.DiscordApi.Expect(adding ? "PUT" : "DELETE", $"guilds/{guild.Id}/members/{user.Id}/roles/{role}",
            new { code = discordCode, message = "sensitive diagnostic" },
            discordCode == 10011 ? HttpStatusCode.NotFound : HttpStatusCode.Forbidden);
        scenario.Logs.ExpectError<BirthdayRoleDomainService>(adding
            ? "Exception occurred when adding birthday role"
            : "Exception occurred when removing birthday role");
    }
}
