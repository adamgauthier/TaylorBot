using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Heist.Commands;

[Name("Heist 💰")]
public class HeistModule(ICommandRunner commandRunner, HeistPlaySlashCommand heistCommand) : TaylorBotModule
{
    [Command(HeistPlaySlashCommand.PrefixCommandName)]
    [Summary("Get a group together and attempt to heist a taypoint bank")]
    public async Task<RuntimeResult> HeistAsync(
        [Summary("How much of your taypoints do you want to invest into heist equipment?")]
        [Remainder]
        string amount
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            heistCommand.Heist(context, amount: null, amountString: amount),
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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </heist profile:1183612687935078512> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("rankheistwins")]
    [Alias("rank heistwins")]
    [Summary("This command has been moved to </heist leaderboard:1183612687935078512>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RankHeistWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </heist leaderboard:1183612687935078512> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
