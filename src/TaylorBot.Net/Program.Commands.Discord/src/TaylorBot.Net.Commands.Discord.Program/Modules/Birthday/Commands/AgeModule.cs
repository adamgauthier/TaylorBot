using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

[Name("Age")]
public class AgeModule(ICommandRunner commandRunner, PrefixedCommandRunner prefixedCommandRunner, BirthdayShowSlashCommand birthdayShowCommand, CommandMentioner mention) : TaylorBotModule
{
    [Command("age")]
    public async Task<RuntimeResult> ShowAgeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: BirthdayAgeSlashCommand.CommandName, IsRemoved: true));

    [Command("setage")]
    [Alias("set age")]
    public async Task<RuntimeResult> SetAgeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true, RemovedMessage:
            $"""
            Setting age directly is not supported, please use {mention.SlashCommand("birthday set")} with the **year** option ⚠️
            This way, your age will automatically update and you will get points on your birthday every year! 🎈
            If you don't want to share your exact birthday, but want the points, horoscope and age commands, use {mention.SlashCommand("birthday set")} with the **privately** option 🕵️‍
            """));

    [Command("clearage")]
    [Alias("clear age")]
    public async Task<RuntimeResult> ClearAgeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true, RemovedMessage: $"Your age is associated with your set birthday, you can use 👉 {mention.SlashCommand("birthday clear")} 👈 to clear it 😊"));

    [Command("birthday")]
    [Alias("bd", "bday")]
    public async Task<RuntimeResult> BirthdayAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: BirthdayShowSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            birthdayShowCommand.Birthday(new(u), context.CreatedAt, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setbirthday")]
    [Alias("set birthday", "setbd", "set bd", "setbday", "set bday")]
    public async Task<RuntimeResult> SetBirthdayAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: BirthdaySetSlashCommand.CommandName, IsRemoved: true));

    [Command("clearbirthday")]
    [Alias("clear birthday", "clearbd", "clear bd", "clearbday", "clear bday")]
    public async Task<RuntimeResult> ClearBirthdayAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: BirthdayClearSlashCommand.CommandName, IsRemoved: true));
}
