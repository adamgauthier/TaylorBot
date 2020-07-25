using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules
{
    [Name("Last.fm 🎶")]
    [Group("lastfm")]
    [Alias("fm", "np")]
    public class LastFmModule : TaylorBotModule
    {
        private readonly IOptionsMonitor<LastFmOptions> _options;
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly ILastFmClient _lastFmClient;

        public LastFmModule(IOptionsMonitor<LastFmOptions> options, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient)
        {
            _options = options;
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmClient = lastFmClient;
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
            var u = user == null ? Context.User : await user.GetTrackedUserAsync();

            var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(u);

            var embed = new EmbedBuilder();

            if (lastFmUsername == null)
            {
                return new TaylorBotEmbedResult(embed
                    .WithUserAsAuthor(Context.User)
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription(string.Join('\n', new[] {
                        $"{u.Mention}'s Last.fm username is not set. 🚫",
                        $"Last.fm can track your listening habits on any platform. You can create a Last.fm account by {"clicking here".DiscordMdLink("https://www.last.fm/join")}.",
                        $"You can then set it up on TaylorBot with `{Context.CommandPrefix}lastfm set <username>`."
                    }))
                .Build());
            }

            var result = await _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username);

            switch (result)
            {
                case LastFmErrorResult errorResult:
                    return new TaylorBotEmbedResult(embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n', new[] {
                            $"Last.fm returned an error. ({errorResult.Error}) 😢",
                            "The site might be down. Try again later!"
                        }))
                    .Build());

                case MostRecentScrobbleResult success:
                    embed.WithAuthor(
                        name: lastFmUsername.Username,
                        iconUrl: u.GetAvatarUrlOrDefault(),
                        url: lastFmUsername.LinkToProfile
                    );

                    if (success.MostRecentTrack != null)
                    {
                        var mostRecentTrack = success.MostRecentTrack;

                        if (mostRecentTrack.TrackImageUrl != null)
                        {
                            embed.WithThumbnailUrl(mostRecentTrack.TrackImageUrl);
                        }

                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .AddField("Artist", mostRecentTrack.Artist.Name.DiscordMdLink(mostRecentTrack.Artist.Url), inline: true)
                            .AddField("Track", mostRecentTrack.TrackName.DiscordMdLink(mostRecentTrack.TrackUrl), inline: true)
                            .WithFooter(text: string.Join(" | ", new[] {
                                mostRecentTrack.IsNowPlaying ? "Now Playing" : "Most Recent Track",
                                $"Total Scrobbles: {success.TotalScrobbles}"
                            }), iconUrl: _options.CurrentValue.LastFmEmbedFooterIconUrl);
                    }
                    else
                    {
                        embed
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription(string.Join('\n', new[] {
                                "This account does not have any scrobbles. 🔍",
                                "Start listening to a song and scrobble it to Last.fm so it shows up here!"
                            }));
                    }

                    return new TaylorBotEmbedResult(embed.Build());

                default: throw new NotImplementedException();
            }
        }

        [Command("set")]
        [Summary("Registers your Last.fm username for use with other Last.fm commands.")]
        public async Task<RuntimeResult> SetAsync(
            [Summary("What is your Last.fm username?")]
            LastFmUsername lastFmUsername
        )
        {
            await _lastFmUsernameRepository.SetLastFmUsernameAsync(Context.User, lastFmUsername);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(string.Join('\n', new[] {
                    $"Your Last.fm username has been set to {lastFmUsername.Username.DiscordMdLink(lastFmUsername.LinkToProfile)}. ✅",
                    $"You can now use Last.fm commands, get started with `{Context.CommandPrefix}lastfm`."
                }))
            .Build());
        }

        [Command("clear")]
        [Summary("Clears your registered Last.fm username.")]
        public async Task<RuntimeResult> ClearAsync()
        {
            await _lastFmUsernameRepository.ClearLastFmUsernameAsync(Context.User);

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(string.Join('\n', new[] {
                    $"Your Last.fm username has been cleared. Last.fm commands will no longer work. ✅",
                    $"You can set it again with `{Context.CommandPrefix}lastfm set <username>`."
                }))
            .Build());
        }
    }
}
