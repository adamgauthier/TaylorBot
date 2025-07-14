using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

[Name("Last.fm 🎶")]
[Group("lastfm")]
[Alias("fm", "np")]
public class LastFmModule(
    ICommandRunner commandRunner,
    LastFmCurrentSlashCommand lastFmCurrentCommand,
    LastFmSetSlashCommand lastFmSetCommand,
    LastFmClearSlashCommand lastFmClearCommand,
    LastFmTracksSlashCommand lastFmTracksCommand,
    LastFmAlbumsSlashCommand lastFmAlbumsCommand,
    LastFmArtistsSlashCommand lastFmArtistsCommand,
    PrefixedCommandRunner prefixedCommandRunner
    ) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    public async Task<RuntimeResult> NowPlayingAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmCurrentSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmCurrentCommand.Current(new(user == null ? Context.User : await user.GetTrackedUserAsync()), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("set")]
    public async Task<RuntimeResult> SetAsync(
        LastFmUsername lastFmUsername
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmSetSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmSetCommand.Set(context.User, lastFmUsername, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("clear")]
    public async Task<RuntimeResult> ClearAsync()
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmClearSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmClearCommand.Clear(context.User, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("artists")]
    public async Task<RuntimeResult> ArtistsAsync(
        LastFmPeriod? period = null,
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmArtistsSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmArtistsCommand.Artists(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("tracks")]
    public async Task<RuntimeResult> TracksAsync(
        LastFmPeriod? period = null,
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmTracksSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmTracksCommand.Tracks(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("albums")]
    public async Task<RuntimeResult> AlbumsAsync(
        LastFmPeriod? period = null,
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LastFmAlbumsSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmAlbumsCommand.Albums(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("collage")]
    [Alias("c")]
    public async Task<RuntimeResult> CollageAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: LastFmCollageSlashCommand.CommandName, IsRemoved: true));
}


[Name("Last.fm old 🎶")]
public class LastFmDeprecatedModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("setlastfm")]
    [Alias("setfm")]
    public async Task<RuntimeResult> SetFmAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: LastFmSetSlashCommand.CommandName, IsRemoved: true));

    [Command("clearlastfm")]
    [Alias("clearfm")]
    public async Task<RuntimeResult> ClearFmAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: LastFmClearSlashCommand.CommandName, IsRemoved: true));

    [Command("lastfmcollage")]
    [Alias("fmcollage", "fmc")]
    public async Task<RuntimeResult> FmcAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: LastFmCollageSlashCommand.CommandName, IsRemoved: true));
}
