using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Commands
{
    public class WeatherCommand
    {
        public static readonly CommandMetadata Metadata = new("weather", "Weather 🌦");

        private readonly IRateLimiter _rateLimiter;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationClient _locationClient;
        private readonly IWeatherClient _weatherClient;

        public WeatherCommand(IRateLimiter rateLimiter, ILocationRepository locationRepository, ILocationClient locationClient, IWeatherClient weatherClient)
        {
            _rateLimiter = rateLimiter;
            _locationRepository = locationRepository;
            _locationClient = locationClient;
            _weatherClient = weatherClient;
        }

        public Command Weather(IUser author, IUser user, string? locationOverride, string commandPrefix) => new(
            Metadata,
            async () =>
            {
                Location location;

                if (locationOverride != null)
                {
                    var placeRateLimit = await _rateLimiter.VerifyDailyLimitAsync(author, "google-places-search");
                    if (placeRateLimit != null)
                        return placeRateLimit;

                    var locationResult = await _locationClient.GetLocationAsync(locationOverride);
                    switch (locationResult)
                    {
                        case LocationGenericErrorResult _:
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"Unexpected error happened when attempting to find this location. 😢",
                                $"The location service might be down. Try again later!"
                            })));

                        case LocationNotFoundResult _:
                            return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"Unable to find the location you specified. 🔍",
                                $"Are you sure it's a real place that exist in the world?"
                            })));

                        case LocationFoundResult found:
                            location = new(found.Location.Latitude, found.Location.Longitude, found.Location.FormattedAddress);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
                else
                {
                    var storedLocation = await _locationRepository.GetLocationAsync(user);
                    if (storedLocation == null)
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"{user.Mention}'s location is not set. 🚫",
                            $"They need to use `{commandPrefix}setlocation` to set it first."
                        })));
                    }
                    location = storedLocation;
                }

                var weatherRateLimit = await _rateLimiter.VerifyDailyLimitAsync(author, "weather-report");
                if (weatherRateLimit != null)
                    return weatherRateLimit;

                var result = await _weatherClient.GetCurrentForecastAsync(location.Latitude, location.Longitude);

                switch (result)
                {
                    case CurrentForecast forecast:
                        var embed = new EmbedBuilder()
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithUserAsAuthor(user)
                            .WithTitle(forecast.Summary)
                            .WithDescription(string.Join('\n', new[] {
                                $"{forecast.TemperatureCelsius:0.#}°C/{ConvertCelsiusToFahrenheit(forecast.TemperatureCelsius):0.#}°F",
                                $"Wind: {forecast.WindSpeed} m/s",
                                $"Humidity: {Math.Round(forecast.Humidity * 100)}%"
                            }))
                            .WithFooter(location.FormattedAddress)
                            .WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(forecast.Time));

                        if (forecast.IconUrl != null)
                        {
                            embed.WithThumbnailUrl(forecast.IconUrl);
                        }

                        return new EmbedResult(embed.Build());

                    case WeatherGenericErrorResult _:
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"Unexpected error happened when attempting to get a forecast. 😢",
                            $"The weather service might be down. Try again later!"
                        })));

                    default: throw new NotImplementedException();
                }
            }
        );

        private static double ConvertCelsiusToFahrenheit(double celsius)
        {
            return celsius * 9d / 5d + 32d;
        }
    }

    public class WeatherSlashCommand : ISlashCommand<WeatherSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("weather");

        public record Options(ParsedUserOrAuthor user, ParsedOptionalString location);

        private readonly WeatherCommand _weatherCommand;

        public WeatherSlashCommand(WeatherCommand weatherCommand)
        {
            _weatherCommand = weatherCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _weatherCommand.Weather(context.User, options.user.User, options.location.Value, context.CommandPrefix)
            );
        }
    }
}
