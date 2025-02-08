using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

[Name("Last.fm 🎶")]
[Group("lastfm")]
[Alias("fm", "np")]
public class LastFmModule(
    ICommandRunner commandRunner,
    LastFmCurrentCommand lastFmCurrentCommand,
    LastFmSetCommand lastFmSetCommand,
    LastFmClearCommand lastFmClearCommand,
    LastFmTracksCommand lastFmTracksCommand,
    LastFmAlbumsCommand lastFmAlbumsCommand,
    LastFmArtistsCommand lastFmArtistsCommand
    ) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("Displays the currently playing or most recently played track for a user's Last.fm profile.")]
    public async Task<RuntimeResult> NowPlayingAsync(
        [Summary("What user would you like to see the now playing of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmCurrentCommand.Current(new(user == null ? Context.User : await user.GetTrackedUserAsync())),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("set")]
    [Summary("Registers your Last.fm username for use with other Last.fm commands.")]
    public async Task<RuntimeResult> SetAsync(
        [Summary("What is your Last.fm username?")]
        LastFmUsername lastFmUsername
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmSetCommand.Set(context.User, lastFmUsername, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("clear")]
    [Summary("Clears your registered Last.fm username.")]
    public async Task<RuntimeResult> ClearAsync()
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmClearCommand.Clear(context.User, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("artists")]
    [Summary("Gets the top artists listened to by a user over a period.")]
    public async Task<RuntimeResult> ArtistsAsync(
        [Summary("What period of time would you like the top artists for?")]
        LastFmPeriod? period = null,
        [Summary("What user would you like to see a the top artists for?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmArtistsCommand.Artists(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("tracks")]
    [Summary("Gets the top tracks listened to by a user over a period.")]
    public async Task<RuntimeResult> TracksAsync(
        [Summary("What period of time would you like the top tracks for?")]
        LastFmPeriod? period = null,
        [Summary("What user would you like to see a the top tracks for?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmTracksCommand.Tracks(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("albums")]
    [Summary("Gets the top albums listened to by a user over a period.")]
    public async Task<RuntimeResult> AlbumsAsync(
        [Summary("What period of time would you like the top albums for?")]
        LastFmPeriod? period = null,
        [Summary("What user would you like to see a the top albums for?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            lastFmAlbumsCommand.Albums(period, new(user == null ? Context.User : await user.GetTrackedUserAsync()), isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("collage")]
    [Alias("c")]
    [Summary("This command has been moved to </lastfm collage:922354806574678086>, please use it instead.")]
    public Task<RuntimeResult> CollageAsync(
        [Remainder]
        string? _ = null
    )
    {
        return Task.FromResult<RuntimeResult>(new TaylorBotResult(
            new EmbedResult(EmbedFactory.CreateError($"This command has been moved to </lastfm collage:922354806574678086>, please use it instead.")),
            DiscordNetContextMapper.MapToRunContext(Context)
        ));
    }
}


[Name("Last.fm old 🎶")]
public class LastFmDeprecatedModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("setlastfm")]
    [Alias("setfm")]
    [Summary("This command has been moved to </lastfm set:922354806574678086>. Please use it instead! 😊")]
    public async Task<RuntimeResult> SetFmAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </lastfm set:922354806574678086> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearlastfm")]
    [Alias("clearfm")]
    [Summary("This command has been moved to </lastfm clear:922354806574678086>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearFmAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </lastfm clear:922354806574678086> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("lastfmcollage")]
    [Alias("fmcollage", "fmc")]
    [Summary("This command has been moved to </lastfm collage:922354806574678086>. Please use it instead! 😊")]
    public async Task<RuntimeResult> FmcAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </lastfm collage:922354806574678086> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
