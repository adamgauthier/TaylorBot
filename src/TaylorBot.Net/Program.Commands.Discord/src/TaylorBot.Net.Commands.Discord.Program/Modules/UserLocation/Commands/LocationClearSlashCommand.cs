using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

public class LocationClearSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("location clear");

    private readonly ILocationRepository _locationRepository;

    public LocationClearSlashCommand(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _locationRepository.ClearLocationAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your location has been cleared. {context.MentionCommand("location time")} and {context.MentionCommand("location weather")} will no longer work. ✅
                    You can set it again with {context.MentionCommand("location set")}.
                    """));
            }
        ));
    }
}
