using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Birthday;

[Trait("Command", "birthday horoscope")]
public sealed class HoroscopeTests(DataServices data)
{
    [Fact]
    public async Task Horoscope_UsesBirthdayZodiacAndParsesDailyContent()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(other, new(year: 1989, month: 12, day: 13), isPrivate: true);
        scenario.External.Horoscope();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday horoscope", selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Be("Make time for friends & music.\n\nTry something new.");
        response.Embed.GetProperty("title").GetString().Should().Contain("Sagittarius");
    }

    [Fact]
    public async Task Horoscope_ProviderFailureExplainsRetry()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.BirthdayAsync(user, new(year: 1989, month: 12, day: 13));
        scenario.External.HoroscopeUnavailable();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "birthday horoscope");

        response.ShouldBeError();
        response.Description.Should().Contain("GaneshaSpeaks returned an error").And.Contain("Try again later");
    }
}
