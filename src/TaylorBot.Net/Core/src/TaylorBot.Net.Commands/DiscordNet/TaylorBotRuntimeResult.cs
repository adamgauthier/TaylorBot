using Discord.Commands;

namespace TaylorBot.Net.Commands.DiscordNet;

public class TaylorBotResult(ICommandResult result, RunContext context) : RuntimeResult(error: null, reason: null)
{
    public ICommandResult Result { get; } = result;
    public RunContext Context { get; } = context;
}
