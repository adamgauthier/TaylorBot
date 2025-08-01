﻿using Discord;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationShowCommand(ILocationRepository locationRepository, CommandMentioner mention, TimeProvider timeProvider)
{
    public static readonly CommandMetadata Metadata = new("location show");

    public Command Location(DiscordUser user, RunContext context) => new(
        Metadata,
        async () =>
        {
            var location = await locationRepository.GetLocationAsync(user);

            if (location != null)
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
                var now = TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, timeZone);

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
                    They need to use {mention.SlashCommand("location set", context)} to set it first.
                    """
                ));
            }
        }
    );
}

public class LocationShowSlashCommand(LocationShowCommand locationShowCommand) : ISlashCommand<LocationShowSlashCommand.Options>
{
    public static string CommandName => "location show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(locationShowCommand.Location(options.user.User, context));
    }
}

public class LocationTimeSlashCommand(LocationShowCommand locationShowCommand) : ISlashCommand<LocationShowSlashCommand.Options>
{
    public static string CommandName => "location time";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, LocationShowSlashCommand.Options options)
    {
        return new(locationShowCommand.Location(options.user.User, context));
    }
}
