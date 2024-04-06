using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class WeatherCommand(IRateLimiter rateLimiter, ILocationRepository locationRepository, IWeatherClient weatherClient, LocationFetcherDomainService locationFetcherDomainService)
{
    public static readonly CommandMetadata Metadata = new("location weather", "Location 🌍");

    public Command Weather(DiscordUser author, IUser user, string? locationOverride, RunContext? context = null) => new(
        Metadata,
        async () =>
        {
            Location location;

            if (locationOverride != null)
            {
                var foundLocation = await locationFetcherDomainService.GetLocationAsync(author, locationOverride);
                if (foundLocation)
                {
                    location = foundLocation.Value;
                }
                else
                {
                    return foundLocation.Error;
                }
            }
            else
            {
                var storedLocation = await locationRepository.GetLocationAsync(new(user));
                if (storedLocation == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s location is not set. 🚫
                        They need to use {context?.MentionCommand("location set") ?? "</location set:1141925890448691270>"} to set it first.
                        """
                    ));
                }
                location = storedLocation.Location;
            }

            var weatherRateLimit = await rateLimiter.VerifyDailyLimitAsync(author, "weather-report");
            if (weatherRateLimit != null)
                return weatherRateLimit;

            var result = await weatherClient.GetCurrentForecastAsync(location.Latitude, location.Longitude);

            switch (result)
            {
                case CurrentForecast forecast:
                    var embed = new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithUserAsAuthor(user)
                        .WithTitle(forecast.Summary)
                        .WithDescription(
                            $"""
                            {forecast.TemperatureCelsius:0.#}°C/{ConvertCelsiusToFahrenheit(forecast.TemperatureCelsius):0.#}°F
                            Wind: {forecast.WindSpeed} m/s
                            Humidity: {Math.Round(forecast.Humidity * 100)}%
                            """
                        )
                        .WithFooter(location.FormattedAddress)
                        .WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(forecast.Time));

                    if (forecast.IconUrl != null)
                    {
                        embed.WithThumbnailUrl(forecast.IconUrl);
                    }

                    return new EmbedResult(embed.Build());

                case WeatherGenericErrorResult _:
                    return new EmbedResult(EmbedFactory.CreateError(
                        """
                        Unexpected error happened when attempting to get a forecast. 😢
                        The weather service might be down. Try again later!"
                        """
                    ));

                default: throw new NotImplementedException();
            }
        }
    );

    private static double ConvertCelsiusToFahrenheit(double celsius)
    {
        return celsius * 9d / 5d + 32d;
    }
}

public class WeatherSlashCommand(WeatherCommand weatherCommand) : ISlashCommand<WeatherSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location weather");

    public record Options(ParsedFetchedUserOrAuthor user, ParsedOptionalString location);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            weatherCommand.Weather(context.User, options.user.User, options.location.Value, context)
        );
    }
}
