using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    [Name("Last.fm 🎶")]
    [Group("lastfm")]
    [Alias("fm", "np")]
    public class LastFmModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly LastFmCurrentCommand _lastFmCurrentCommand;
        private readonly LastFmSetCommand _lastFmSetCommand;
        private readonly LastFmClearCommand _lastFmClearCommand;
        private readonly LastFmCollageCommand _lastFmCollageCommand;
        private readonly LastFmTracksCommand _lastFmTracksCommand;
        private readonly LastFmAlbumsCommand _lastFmAlbumsCommand;
        private readonly LastFmArtistsCommand _lastFmArtistsCommand;

        public LastFmModule(
            ICommandRunner commandRunner,
            LastFmCurrentCommand lastFmCurrentCommand,
            LastFmSetCommand lastFmSetCommand,
            LastFmClearCommand lastFmClearCommand,
            LastFmCollageCommand lastFmCollageCommand,
            LastFmTracksCommand lastFmTracksCommand,
            LastFmAlbumsCommand lastFmAlbumsCommand,
            LastFmArtistsCommand lastFmArtistsCommand,
            LastFmEmbedFactory lastFmEmbedFactory
        )
        {
            _commandRunner = commandRunner;
            _lastFmCurrentCommand = lastFmCurrentCommand;
            _lastFmSetCommand = lastFmSetCommand;
            _lastFmClearCommand = lastFmClearCommand;
            _lastFmCollageCommand = lastFmCollageCommand;
            _lastFmTracksCommand = lastFmTracksCommand;
            _lastFmAlbumsCommand = lastFmAlbumsCommand;
            _lastFmArtistsCommand = lastFmArtistsCommand;
        }

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
            var result = await _commandRunner.RunAsync(
                _lastFmCurrentCommand.Current(user == null ? Context.User : await user.GetTrackedUserAsync()),
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
            var result = await _commandRunner.RunAsync(
                _lastFmSetCommand.Set(context.User, lastFmUsername, isLegacyCommand: true),
                context
            );

            return new TaylorBotResult(result, context);
        }

        [Command("clear")]
        [Summary("Clears your registered Last.fm username.")]
        public async Task<RuntimeResult> ClearAsync()
        {
            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(
                _lastFmClearCommand.Clear(context.User, isLegacyCommand: true),
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
            var result = await _commandRunner.RunAsync(
                _lastFmArtistsCommand.Artists(period, user == null ? Context.User : await user.GetTrackedUserAsync(), isLegacyCommand: true),
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
            var result = await _commandRunner.RunAsync(
                _lastFmTracksCommand.Tracks(period, user == null ? Context.User : await user.GetTrackedUserAsync(), isLegacyCommand: true),
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
            var result = await _commandRunner.RunAsync(
                _lastFmAlbumsCommand.Albums(period, user == null ? Context.User : await user.GetTrackedUserAsync(), isLegacyCommand: true),
                context
            );

            return new TaylorBotResult(result, context);
        }

        [Command("collage")]
        [Alias("c")]
        [Summary("Generates a collage based on a user's Last.fm listening habits. Collages are provided by a third-party and might have loading problems.")]
        public async Task<RuntimeResult> CollageAsync(
            [Summary("What period of time would you like the collage for?")]
            LastFmPeriod? period = null,
            [Summary("What size (number of rows and columns) would you like the collage to be?")]
            LastFmCollageSize? size = null,
            [Summary("What user would you like to see a collage for?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(
                _lastFmCollageCommand.Collage(period, size, user == null ? Context.User : await user.GetTrackedUserAsync(), isLegacyCommand: true),
                context
            );

            return new TaylorBotResult(result, context);
        }
    }
}
