using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationSetSlashCommand(ILocationClient locationClient, ILocationRepository locationRepository, LocationFetcherDomainService locationFetcherDomainService, CommandMentioner mention) : ISlashCommand<LocationSetSlashCommand.Options>
{
    public static string CommandName => "location set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public record Options(ParsedString location);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new ValueTask<Command>(new Command(
            new(Info.Name),
            async () =>
            {
                var foundLocation = await locationFetcherDomainService.GetLocationAsync(context.User, options.location.Value);
                if (foundLocation)
                {
                    var location = foundLocation.Value;

                    var timeZoneResult = await locationClient.GetTimeZoneForLocationAsync(location.Latitude, location.Longitude);
                    switch (timeZoneResult)
                    {
                        case TimeZoneGenericErrorResult _:
                            return new EmbedResult(EmbedFactory.CreateError(
                                """
                                Unexpected error happened when attempting to find information about this location 😢
                                The location service might be down. Try again later!
                                """));

                        case TimeZoneResult timeZone:
                            await locationRepository.SetLocationAsync(context.User, new(location, timeZone.TimeZoneId));
                            return new EmbedResult(EmbedFactory.CreateSuccess(
                                $"""
                                Your location has been set to **{location.FormattedAddress}** 🌍
                                You can use {mention.SlashCommand("location weather", context)} to see the current weather at your location 🌦
                                People can now use {mention.SlashCommand("location time", context)} to see what time it is for you 🕰️
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
