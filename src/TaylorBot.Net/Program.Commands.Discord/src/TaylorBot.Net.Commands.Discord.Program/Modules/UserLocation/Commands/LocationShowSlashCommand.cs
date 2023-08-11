using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationShowCommand
{
    public static readonly CommandMetadata Metadata = new("location show", "Location 🌍");

    private readonly ILocationRepository _locationRepository;

    public LocationShowCommand(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public Command Location(IUser user, RunContext? context = null) => new(
        Metadata,
        async () =>
        {
            var location = await _locationRepository.GetLocationAsync(user);

            if (location != null)
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(user)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        {user.Username}'s location is **{location.Location.FormattedAddress}**. 🌍
                        It is currently **{now:h:mm tt}** there ({timeZone.StandardName}). 🕰️
                        """);

                return new EmbedResult(embed.Build());
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""                        
                    {user.Mention}'s location is not set. 🚫
                    They need to use {context?.MentionCommand("location set") ?? "**/location set**"} to set it first.
                    """
                ));
            }
        }
    );
}

public class LocationShowSlashCommand : ISlashCommand<LocationShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location show");

    public record Options(ParsedUserOrAuthor user);

    private readonly LocationShowCommand _locationShowCommand;

    public LocationShowSlashCommand(LocationShowCommand locationShowCommand)
    {
        _locationShowCommand = locationShowCommand;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(_locationShowCommand.Location(options.user.User, context));
    }
}

public class LocationTimeSlashCommand : ISlashCommand<LocationShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location time");

    private readonly LocationShowCommand _locationShowCommand;

    public LocationTimeSlashCommand(LocationShowCommand locationShowCommand)
    {
        _locationShowCommand = locationShowCommand;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, LocationShowSlashCommand.Options options)
    {
        return new(_locationShowCommand.Location(options.user.User, context));
    }
}
