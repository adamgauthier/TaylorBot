namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class MemberLoggingTests(DataServices data)
{
    [Fact]
    public async Task JoinLeaveAndRejoin_DrainNestedMemberLogs()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var owner = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(owner);
        var joining = await scenario.Given.UserAsync(username: "Joining");
        await scenario.Given.LogChannelAsync(guild, "members");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var joined = await scenario.Discord.JoinAsync(guild, joining);
        var left = await scenario.Discord.LeaveAsync(guild, joining);
        var rejoined = await scenario.Discord.JoinAsync(guild, joining);

        joined.Messages.Should().ContainSingle();
        left.Messages.Should().ContainSingle();
        rejoined.Messages.Should().ContainSingle();
        rejoined.Messages.Single().Body!.Value.GetRawText().Should().Contain(joining.Id);
        (await scenario.State.MemberAsync(guild, joining)).Alive.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BanEvent_LogsAffectedMember(bool unban)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LogChannelAsync(guild, "members");
        scenario.DiscordApi.ExpectMessage(guild.ChannelId);

        var output = await scenario.Discord.BanAsync(guild, user, unban);

        output.Messages.Single().Body!.Value.GetRawText().Should().Contain(user.Id);
    }
}
