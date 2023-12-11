using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

[Name("Heist 💰")]
public class HeistModule(ICommandRunner commandRunner, HeistPlaySlashCommand heistCommand) : TaylorBotModule
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

    [Command("heistwins")]
    [Alias("hwins", "heistprofits", "hprofits", "heistfails", "hfails", "heistlosses", "hlosses")]
    [Summary("This command has been moved to </heist profile:1183612687935078512>. Please use it instead! 😊")]
    public async Task<RuntimeResult> HeistWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </heist profile:1183612687935078512> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
