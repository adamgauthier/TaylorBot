using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class WeatherCommandTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();

    private readonly ILocationRepository _locationRepository = A.Fake<ILocationRepository>(o => o.Strict());
    private readonly ILocationClient _locationClient = A.Fake<ILocationClient>(o => o.Strict());
    private readonly IWeatherClient _weatherClient = A.Fake<IWeatherClient>(o => o.Strict());
    private readonly WeatherCommand _weatherCommand;

    public WeatherCommandTests()
    {
        _weatherCommand = new WeatherCommand(
            CommandUtils.UnlimitedRateLimiter, _locationRepository, _weatherClient, new(CommandUtils.UnlimitedRateLimiter, _locationClient));
    }

    [Fact]
    public async Task Weather_ThenReturnsEmbedWithTemperature()
    {
        const double Temperature = 13.3;
        Location location = new("46.8130816", "-71.20745959999999", "Québec City, QC, Canada");
        A.CallTo(() => _locationRepository.GetLocationAsync(_commandUser)).Returns(new StoredLocation(location, ""));
        A.CallTo(() => _weatherClient.GetCurrentForecastAsync(location.Latitude, location.Longitude)).Returns(new CurrentForecast("", Temperature, 0, 0, 0, ""));

        var result = (EmbedResult)await _weatherCommand.Weather(_commandUser, _commandUser, locationOverride: null).RunAsync();

        result.Embed.Description.Should().Contain($"{Temperature}°C");
        result.Embed.Footer?.Text.Should().Contain(location.FormattedAddress);
    }
}
