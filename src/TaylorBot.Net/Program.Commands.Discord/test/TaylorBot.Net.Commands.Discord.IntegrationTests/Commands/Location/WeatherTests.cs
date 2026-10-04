using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Location;

[Trait("Command", "location weather")]
public sealed class WeatherTests(DataServices data)
{
    [Fact]
    public async Task Forecast_ShowsStoredLocationAndBothUnitSystems()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LocationAsync(user);
        scenario.External.Weather();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "location weather");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("13.3\u00b0C").And.Contain("55.9\u00b0F")
            .And.Contain("5.5 m/s").And.Contain("12.3 mph").And.Contain("61%");
        response.Embed.GetProperty("footer").GetProperty("text").GetString().Should().Be("Quebec City, QC, Canada");
    }
}
