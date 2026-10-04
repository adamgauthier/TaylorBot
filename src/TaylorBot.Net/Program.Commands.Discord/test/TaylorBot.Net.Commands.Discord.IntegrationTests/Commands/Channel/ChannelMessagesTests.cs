using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Channel;

[Trait("Command", "channel messages")]
public sealed class ChannelMessagesTests(DataServices data)
{
    [Theory]
    [InlineData(false, "not considered as spam")]
    [InlineData(true, "NOT")]
    public async Task Messages_ShowsCountAndSpamTrackingPolicy(bool spam, string expectedPolicy)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ChannelMessagesAsync(guild, spam);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "channel messages", guild);

        response.ShouldBeSuccess();
        response.Field("Message Count").Replace(",", "", StringComparison.Ordinal).Should().Contain("1234");
        response.Field("Spam Status").Should().Contain(expectedPolicy);
    }

    [Fact]
    public async Task Messages_ExplicitChannelResolvesCount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.ChannelMessagesAsync(guild, spam: false);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "channel messages", guild, arguments: [SlashArgument.Channel("channel", guild)]);

        response.ShouldBeSuccess();
        response.Field("Message Count").Replace(",", "", StringComparison.Ordinal).Should().Contain("1234");
        response.Field("Spam Status").Should().Contain("not considered as spam");
    }

    [Fact]
    public async Task Messages_DirectMessageRequiresServer()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "channel messages");

        response.ShouldBeError();
        response.Description.Should().Contain("server");
    }
}
