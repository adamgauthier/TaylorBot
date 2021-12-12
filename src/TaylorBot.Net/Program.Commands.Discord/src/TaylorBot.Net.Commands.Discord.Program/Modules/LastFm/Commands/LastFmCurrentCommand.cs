using Discord;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmCurrentCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm current", "Last.fm 🎶", new[] { "fm", "np" });

        private readonly IOptionsMonitor<LastFmOptions> _options;
        private readonly LastFmEmbedFactory _lastFmEmbedFactory;
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly ILastFmClient _lastFmClient;

        public LastFmCurrentCommand(IOptionsMonitor<LastFmOptions> options, LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient)
        {
            _options = options;
            _lastFmEmbedFactory = lastFmEmbedFactory;
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmClient = lastFmClient;
        }

        public Command Current(IUser user) => new(
            Metadata,
            async () =>
            {
                var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                if (lastFmUsername == null)
                    return _lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                var result = await _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username);

                switch (result)
                {
                    case MostRecentScrobbleResult success:
                        if (success.MostRecentTrack != null)
                        {
                            var embed = _lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user);

                            var mostRecentTrack = success.MostRecentTrack;

                            if (mostRecentTrack.TrackImageUrl != null)
                            {
                                embed.WithThumbnailUrl(mostRecentTrack.TrackImageUrl);
                            }

                            return new EmbedResult(embed
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
                            return _lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, LastFmPeriod.Overall);
                        }

                    case LastFmLogInRequiredErrorResult _:
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            "Last.fm says your recent tracks are not public. 😢",
                            $"Make sure 'Hide recent listening information' is off in your {"Last.fm privacy settings".DiscordMdLink("https://www.last.fm/settings/privacy")}!"
                        })));

                    case LastFmGenericErrorResult errorResult:
                        return _lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

                    default: throw new NotImplementedException();
                }
            }
        );
    }

    public class LastFmCurrentSlashCommand : ISlashCommand<LastFmCurrentSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("lastfm current");

        public record Options(ParsedUserOrAuthor user);

        private readonly LastFmCurrentCommand _lastFmCurrentCommand;

        public LastFmCurrentSlashCommand(LastFmCurrentCommand lastFmCurrentCommand)
        {
            _lastFmCurrentCommand = lastFmCurrentCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _lastFmCurrentCommand.Current(options.user.User)
            );
        }
    }
}
