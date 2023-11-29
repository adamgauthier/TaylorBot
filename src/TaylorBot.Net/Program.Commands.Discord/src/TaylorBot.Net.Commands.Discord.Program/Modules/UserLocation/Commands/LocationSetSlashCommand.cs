using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationSetSlashCommand : ISlashCommand<LocationSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location set");

    public record Options(ParsedString location);

    private readonly ILocationClient _locationClient;
    private readonly ILocationRepository _locationRepository;
    private readonly LocationFetcherDomainService _locationFetcherDomainService;

    public LocationSetSlashCommand(ILocationClient locationClient, ILocationRepository locationRepository, LocationFetcherDomainService locationFetcherDomainService)
    {
        _locationClient = locationClient;
        _locationRepository = locationRepository;
        _locationFetcherDomainService = locationFetcherDomainService;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new ValueTask<Command>(new Command(
            new(Info.Name),
            async () =>
            {
                var foundLocation = await _locationFetcherDomainService.GetLocationAsync(context.User, options.location.Value);
                if (foundLocation)
                {
                    var location = foundLocation.Value;

                    var timeZoneResult = await _locationClient.GetTimeZoneForLocationAsync(location.Latitude, location.Longitude);
                    switch (timeZoneResult)
                    {
                        case TimeZoneGenericErrorResult _:
                            return new EmbedResult(EmbedFactory.CreateError(
                                """
                                Unexpected error happened when attempting to find information about this location. 😢
                                The location service might be down. Try again later!
                                """));

                        case TimeZoneResult timeZone:
                            await _locationRepository.SetLocationAsync(context.User, new(location, timeZone.TimeZoneId));
                            return new EmbedResult(EmbedFactory.CreateSuccess(
                                $"""
                                Your location has been set to **{location.FormattedAddress}**. 🌍
                                You can use {context.MentionCommand("location weather")} to see the current weather at your location. 🌦
                                People can now use {context.MentionCommand("location time")} to see what time it is for you. 🕰️
                                """));

                        default: throw new NotImplementedException();
                    }
                }
                else
                {
                    return foundLocation.Error;
                }
            }
        ));
    }
}
