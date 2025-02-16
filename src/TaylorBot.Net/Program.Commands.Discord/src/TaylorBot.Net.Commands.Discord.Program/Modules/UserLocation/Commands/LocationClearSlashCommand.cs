using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationClearSlashCommand(ILocationRepository locationRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "location clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await locationRepository.ClearLocationAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your location has been cleared. {context.MentionSlashCommand("location time")} and {context.MentionSlashCommand("location weather")} will no longer work. ✅
                    You can set it again with {context.MentionSlashCommand("location set")}.
                    """));
            }
        ));
    }
}
