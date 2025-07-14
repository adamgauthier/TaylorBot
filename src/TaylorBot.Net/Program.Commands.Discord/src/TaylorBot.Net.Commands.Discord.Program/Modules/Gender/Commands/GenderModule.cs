using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

[Name("Gender")]
public class GenderModule(ICommandRunner commandRunner, GenderShowSlashCommand genderShowCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(GenderShowSlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> ShowGenderAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: GenderShowSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            genderShowCommand.Show(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setgender")]
    [Alias("set gender")]
    public async Task<RuntimeResult> SetGenderAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: GenderSetSlashCommand.CommandName, IsRemoved: true));

    [Command("cleargender")]
    [Alias("clear gender")]
    public async Task<RuntimeResult> ClearGenderAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: GenderClearSlashCommand.CommandName, IsRemoved: true));
}
