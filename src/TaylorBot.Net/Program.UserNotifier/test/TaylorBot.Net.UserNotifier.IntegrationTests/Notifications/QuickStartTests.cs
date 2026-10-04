namespace TaylorBot.Net.UserNotifier.IntegrationTests.Notifications;

public sealed class QuickStartTests(DataServices data)
{
    [Fact]
    public async Task MultipleChannels_PrefersGeneralAndSendsWelcome()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var guild = await scenario.Given.GuildAsync(user, channels: [new("memes"), new("general"), new("serious"), new("commands")]);

        var welcome = scenario.DiscordApi.Requests.Single(request => request.Path.EndsWith("/messages", StringComparison.Ordinal));
        welcome.Path.Should().Be($"channels/{guild.Channels["general"]}/messages");
        welcome.Body!.Value.GetProperty("embeds")[0].GetProperty("description").GetString().Should().Contain("Welcome");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task UnwritableGeneral_UsesAvailableChannel(bool everyoneCanSend, bool botCanSend)
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var guild = await scenario.Given.GuildAsync(user, channels: [new("general", everyoneCanSend, botCanSend), new("chat")], welcomeChannel: "chat");

        scenario.DiscordApi.Requests.Should().Contain(request => request.Path == $"channels/{guild.Channels["chat"]}/messages");
    }

    [Fact]
    public async Task NoWritableChannels_SendsWelcomeToOwner()
    {
        await using var scenario = await UserNotifierScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.DiscordApi.ExpectMessage(Discord.NotifierDiscordApi.DmChannel(user.Id));

        await scenario.Given.GuildAsync(user, channels: [new("general", BotCanSend: false)], welcomeChannel: null);

        scenario.DiscordApi.Requests.Should().Contain(request => request.Path == $"channels/{Discord.NotifierDiscordApi.DmChannel(user.Id)}/messages");
    }
}
