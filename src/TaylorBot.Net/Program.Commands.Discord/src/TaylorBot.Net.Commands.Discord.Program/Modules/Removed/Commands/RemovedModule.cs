using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Removed.Commands;

[Name("Removed 👋")]
public class RemovedModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("instagram")]
    [Alias("insta", "setinstagram", "set instagram", "setinsta", "set insta", "clearinstagram", "clear instagram", "clearinsta", "clear insta")]
    [Summary("This command has been removed, sorry! 😕")]
    public async Task<RuntimeResult> InstagramAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been removed, sorry! 😕
                Please use Discord's profile connections feature instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("snapchat")]
    [Alias("snap", "setsnapchat", "set snapchat", "setsnap", "set snap", "clearsnapchat", "clear snapchat", "clearsnap", "clear snap", "listsnapchat", "list snapchat", "listsnap", "list snap")]
    [Summary("This command has been removed, sorry! 😕")]
    public async Task<RuntimeResult> SnapchatAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError("This command has been removed, sorry! 😕"))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("tumblr")]
    [Alias("settumblr", "set tumblr", "cleartumblr", "clear tumblr", "listtumblr", "list tumblr")]
    [Summary("This command has been removed, sorry! 😕")]
    public async Task<RuntimeResult> TumblrAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError("This command has been removed, sorry! 😕"))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("wikipedia")]
    [Alias("wiki")]
    [Summary("This command has been removed, sorry! 😕")]
    public async Task<RuntimeResult> WikipediaAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError("This command has been removed, sorry! 😕"))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("subreddit")]
    [Alias("sub")]
    [Summary("This command has been removed, sorry! 😕")]
    public async Task<RuntimeResult> SubredditAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError("This command has been removed, sorry! 😕"))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
