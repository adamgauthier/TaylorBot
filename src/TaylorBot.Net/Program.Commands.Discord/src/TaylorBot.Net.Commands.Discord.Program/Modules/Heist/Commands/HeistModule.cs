using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Heist 💰")]
public class HeistModule(ICommandRunner commandRunner, HeistSlashCommand heistCommand) : TaylorBotModule
{
    [Command("heist")]
    [Summary("Get a group together and attempt to heist a taypoint bank")]
    public async Task<RuntimeResult> HeistAsync(
        [Summary("How much of your taypoints do you want to invest into heist equipment?")]
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            heistCommand.Heist(context, Context.User, amount: null, amountString: amount),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
