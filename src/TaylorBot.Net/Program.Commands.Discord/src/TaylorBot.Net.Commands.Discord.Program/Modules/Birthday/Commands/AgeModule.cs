﻿using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

[Name("Age")]
public class AgeModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly BirthdayShowSlashCommand _birthdayShowCommand;

    public AgeModule(ICommandRunner commandRunner, BirthdayShowSlashCommand birthdayShowCommand)
    {
        _commandRunner = commandRunner;
        _birthdayShowCommand = birthdayShowCommand;
    }

    [Command("age")]
    [Summary("This command has been moved to 👉 </birthday age:1016938623880400907> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> ShowAgeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </birthday age:1016938623880400907> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("setage")]
    [Alias("set age")]
    [Summary("Setting age is not supported, use </birthday set:1016938623880400907> with the **year** option instead! 😊")]
    public async Task<RuntimeResult> SetAgeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                Setting age directly is not supported, please use </birthday set:1016938623880400907> with the **year** option. ⚠️
                This way, your age will automatically update and you will get points on your birthday every year! 🎈
                If you don't want to share your exact birthday, but want the points, horoscope and age commands, use </birthday set:1016938623880400907> with the **privately** option. 🕵️‍
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearage")]
    [Alias("clear age")]
    [Summary("Your age is associated with your set birthday, you can use 👉 </birthday clear:1016938623880400907> 👈 to clear it 😊")]
    public async Task<RuntimeResult> ClearAgeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                Your age is associated with your set birthday, you can use 👉 </birthday clear:1016938623880400907> 👈 to clear it 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("birthday")]
    [Alias("bd", "bday")]
    [Summary("Show the birthday of a user")]
    public async Task<RuntimeResult> BirthdayAsync(
        [Summary("What user would you like to see the birthday of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _birthdayShowCommand.Birthday(u, context.CreatedAt, context: null),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setbirthday")]
    [Alias("set birthday", "setbd", "set bd", "setbday", "set bday")]
    [Summary("This command has been moved to 👉 </birthday set:1016938623880400907> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> SetBirthdayAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </birthday set:1016938623880400907> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearbirthday")]
    [Alias("clear birthday", "clearbd", "clear bd", "clearbday", "clear bday")]
    [Summary("This command has been moved to 👉 </birthday clear:1016938623880400907> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearBirthdayAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </birthday clear:1016938623880400907> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
