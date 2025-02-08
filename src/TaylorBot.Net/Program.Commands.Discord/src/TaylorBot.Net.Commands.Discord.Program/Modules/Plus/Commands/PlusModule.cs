﻿using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

[Name("TaylorBot Plus 💎")]
[Group("plus")]
[Alias("support", "patreon", "donate")]
public class PlusModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("This command has been moved to </plus show:1246970937321066608>. Please use it instead! 😊")]
    public async Task<RuntimeResult> PlusAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </plus show:1246970937321066608> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("add")]
    [Summary("This command has been moved to </plus add:1246970937321066608>. Please use it instead! 😊")]
    public async Task<RuntimeResult> AddAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </plus add:1246970937321066608> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("remove")]
    [Summary("This command has been moved to </plus remove:1246970937321066608>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemoveAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </plus remove:1246970937321066608> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
