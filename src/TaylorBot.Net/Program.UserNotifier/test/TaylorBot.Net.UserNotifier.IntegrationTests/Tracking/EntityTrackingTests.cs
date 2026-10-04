namespace TaylorBot.Net.UserNotifier.IntegrationTests.Tracking;

public sealed class EntityTrackingTests(DataServices data)
{
    [Fact]
    public async Task GuildAndUserChanges_KeepNameHistory()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user, name: "Before");

        await scenario.Discord.RenameGuildAsync(guild, "After");
        await scenario.Discord.RenameUserAsync(guild, user, "Renamed");

        (await scenario.State.GuildNamesAsync(guild)).Should().Contain(["Before", "After"]);
        (await scenario.State.UsernamesAsync(user)).Should().Contain("Renamed");
    }

    [Fact]
    public async Task NewTextChannel_IsPersisted()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var channelId = await scenario.Discord.CreateChannelAsync(guild, "New channel");

        (await scenario.State.ChannelExistsAsync(channelId)).Should().BeTrue();
    }

    [Fact]
    public async Task LeaveAndRejoin_UpdateMembership()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        await scenario.Discord.LeaveAsync(guild, user);
        (await scenario.State.MemberAsync(guild, user)).Alive.Should().BeFalse();
        await scenario.Discord.JoinAsync(guild, user);

        (await scenario.State.MemberAsync(guild, user)).Alive.Should().BeTrue();
    }
}
