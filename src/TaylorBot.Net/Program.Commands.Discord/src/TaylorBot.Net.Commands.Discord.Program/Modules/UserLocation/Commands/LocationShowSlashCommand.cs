using Discord;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationShowCommand(ILocationRepository locationRepository)
{
    public static readonly CommandMetadata Metadata = new("location show", "Location 🌍");

    public Command Location(IUser user, RunContext? context = null) => new(
        Metadata,
        async () =>
        {
            var location = await locationRepository.GetLocationAsync(user);

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
                    They need to use {context?.MentionCommand("location set") ?? "</location set:1141925890448691270>"} to set it first.
                    """
                ));
            }
        }
    );
}

public class LocationShowSlashCommand(LocationShowCommand locationShowCommand) : ISlashCommand<LocationShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location show");

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(locationShowCommand.Location(options.user.User, context));
    }
}

public class LocationTimeSlashCommand(LocationShowCommand locationShowCommand) : ISlashCommand<LocationShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location time");

    public ValueTask<Command> GetCommandAsync(RunContext context, LocationShowSlashCommand.Options options)
    {
        return new(locationShowCommand.Location(options.user.User, context));
    }
}
