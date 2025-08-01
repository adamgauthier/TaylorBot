﻿using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class WeatherSlashCommand(
    IRateLimiter rateLimiter,
    ILocationRepository locationRepository,
    IWeatherClient weatherClient,
    LocationFetcherDomainService locationFetcherDomainService,
    CommandMentioner mention
) : ISlashCommand<WeatherSlashCommand.Options>
{
    public static string CommandName => "location weather";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("location weather");

    public record Options(ParsedUserOrAuthor user, ParsedOptionalString location);

    public Command Weather(DiscordUser author, DiscordUser user, string? locationOverride, RunContext context) => new(
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
                var storedLocation = await locationRepository.GetLocationAsync(user);
                if (storedLocation == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        {user.Mention}'s location is not set. 🚫
                        They need to use {mention.SlashCommand("location set", context)} to set it first.
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

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            Weather(context.User, options.user.User, options.location.Value, context)
        );
    }
}
