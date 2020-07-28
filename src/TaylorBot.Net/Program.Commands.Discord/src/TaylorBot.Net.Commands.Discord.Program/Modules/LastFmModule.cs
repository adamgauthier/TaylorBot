using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
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
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;

        public LastFmModule(
            IOptionsMonitor<LastFmOptions> options,
            ILastFmUsernameRepository lastFmUsernameRepository,
            ILastFmClient lastFmClient,
            LastFmPeriodStringMapper lastFmPeriodStringMapper
        )
        {
            _options = options;
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmClient = lastFmClient;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
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

            if (lastFmUsername == null)
                return new TaylorBotEmbedResult(CreateLastFmNotSetEmbed(u));

            var result = await _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username);

            switch (result)
            {
                case LastFmErrorResult errorResult:
                    return new TaylorBotEmbedResult(CreateLastFmErrorEmbed(errorResult));

                case MostRecentScrobbleResult success:
                    if (success.MostRecentTrack != null)
                    {
                        var embed = CreateBaseLastFmEmbed(lastFmUsername, u);

                        var mostRecentTrack = success.MostRecentTrack;

                        if (mostRecentTrack.TrackImageUrl != null)
                        {
                            embed.WithThumbnailUrl(mostRecentTrack.TrackImageUrl);
                        }

                        return new TaylorBotEmbedResult(embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .AddField("Artist", mostRecentTrack.Artist.Name.DiscordMdLink(mostRecentTrack.Artist.Url), inline: true)
                            .AddField("Track", mostRecentTrack.TrackName.DiscordMdLink(mostRecentTrack.TrackUrl), inline: true)
                            .WithFooter(text: string.Join(" | ", new[] {
                                mostRecentTrack.IsNowPlaying ? "Now Playing" : "Most Recent Track",
                                $"Total Scrobbles: {success.TotalScrobbles}"
                            }), iconUrl: _options.CurrentValue.LastFmEmbedFooterIconUrl)
                            .Build()
                        );
                    }
                    else
                    {
                        return new TaylorBotEmbedResult(
                            CreateLastFmNoScrobbleErrorEmbed(lastFmUsername, u)
                        );
                    }

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

        [Command("artists")]
        [Summary("Gets the top artists listened to by a user over a period.")]
        public async Task<RuntimeResult> ArtistsAsync(
            [Summary("What period of time would you like the top artists for?")]
            LastFmPeriod period = LastFmPeriod.SevenDay,
            [Summary("What user would you like to see a the top artists for?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var u = user == null ? Context.User : await user.GetTrackedUserAsync();

            var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(u);

            if (lastFmUsername == null)
                return new TaylorBotEmbedResult(CreateLastFmNotSetEmbed(u));

            var result = await _lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period);

            switch (result)
            {
                case LastFmErrorResult errorResult:
                    return new TaylorBotEmbedResult(CreateLastFmErrorEmbed(errorResult));

                case TopArtistsResult success:
                    if (success.TopArtists.Count > 0)
                    {
                        return new TaylorBotEmbedResult(
                            CreateBaseLastFmEmbed(lastFmUsername, u)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithTitle($"Top artists | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}")
                                .WithDescription(string.Join('\n', success.TopArtists.Select((a, index) =>
                                    $"{index + 1}. {a.Name.DiscordMdLink(a.ArtistUrl.ToString())} - {"play".ToQuantity(a.PlayCount, TaylorBotFormats.BoldReadable)}"
                                )))
                                .Build()
                        );
                    }
                    else
                    {
                        return new TaylorBotEmbedResult(
                            CreateLastFmNoScrobbleErrorEmbed(lastFmUsername, u)
                        );
                    }

                default: throw new NotImplementedException();
            }
        }

        [Command("collage")]
        [Alias("c")]
        [Summary("Generates a collage based on a user's Last.Fm listening habits. Collages are provided by a third-party and might have loading problems.")]
        public async Task<RuntimeResult> CollageAsync(
            [Summary("What period of time would you like the collage for?")]
            LastFmPeriod period = LastFmPeriod.SevenDay,
            [Summary("What size (number of rows and columns) would you like the collage to be?")]
            LastFmCollageSize? size = null,
            [Summary("What user would you like to see a collage for?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var u = user == null ? Context.User : await user.GetTrackedUserAsync();

            if (size == null)
                size = new LastFmCollageSize(3);

            var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(u);

            if (lastFmUsername == null)
                return new TaylorBotEmbedResult(CreateLastFmNotSetEmbed(u));

            var queryString = new[] {
                $"user={lastFmUsername.Username}",
                $"period={_lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
                $"rows={size.Parsed}",
                $"cols={size.Parsed}",
                "imageSize=400",
                $"a={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            };

            return new TaylorBotEmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithAuthor(
                    name: lastFmUsername.Username,
                    iconUrl: u.GetAvatarUrlOrDefault(),
                    url: lastFmUsername.LinkToProfile
                )
                .WithTitle($"Collage {size.Parsed}x{size.Parsed} | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}")
                .WithImageUrl($"https://lastfmtopalbums.dinduks.com/patchwork.php?{string.Join('&', queryString)}")
            .Build());
        }

        private Embed CreateLastFmNotSetEmbed(IUser user)
        {
            return new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription(string.Join('\n', new[] {
                    $"{user.Mention}'s Last.fm username is not set. 🚫",
                    $"Last.fm can track your listening habits on any platform. You can create a Last.fm account by {"clicking here".DiscordMdLink("https://www.last.fm/join")}.",
                    $"You can then set it up on TaylorBot with `{Context.CommandPrefix}lastfm set <username>`."
                }))
            .Build();
        }

        private Embed CreateLastFmErrorEmbed(LastFmErrorResult error)
        {
            return new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription(string.Join('\n', new[] {
                    $"Last.fm returned an error. ({error.Error}) 😢",
                    "The site might be down. Try again later!"
                }))
            .Build();
        }

        private static EmbedBuilder CreateBaseLastFmEmbed(LastFmUsername lastFmUsername, IUser user)
        {
            return new EmbedBuilder().WithAuthor(
                name: lastFmUsername.Username,
                iconUrl: user.GetAvatarUrlOrDefault(),
                url: lastFmUsername.LinkToProfile
            );
        }

        private static Embed CreateLastFmNoScrobbleErrorEmbed(LastFmUsername lastFmUsername, IUser user)
        {
            return CreateBaseLastFmEmbed(lastFmUsername, user)
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription(string.Join('\n', new[] {
                    "This account does not have any scrobbles. 🔍",
                    "Start listening to a song and scrobble it to Last.fm so it shows up here!"
                }))
                .Build();
        }
    }
}
