using Discord;
using Humanizer;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmTracksCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm tracks", "Last.fm 🎶", new[] { "fm tracks", "np tracks" });

        private readonly LastFmEmbedFactory _lastFmEmbedFactory;
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly ILastFmClient _lastFmClient;
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;

        public LastFmTracksCommand(LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient, LastFmPeriodStringMapper lastFmPeriodStringMapper)
        {
            _lastFmEmbedFactory = lastFmEmbedFactory;
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmClient = lastFmClient;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
        }

        public Command Tracks(LastFmPeriod? period, IUser user, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                if (period == null)
                    period = LastFmPeriod.SevenDay;

                var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                if (lastFmUsername == null)
                    return _lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                var result = await _lastFmClient.GetTopTracksAsync(lastFmUsername.Username, period.Value);

                switch (result)
                {
                    case LastFmGenericErrorResult errorResult:
                        return _lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

                    case TopTracksResult success:
                        if (success.TopTracks.Count > 0)
                        {
                            var formattedTracks = success.TopTracks.Select((t, index) =>
                                $"{index + 1}. {t.ArtistName.DiscordMdLink(t.ArtistUrl.ToString())} - {t.Name.DiscordMdLink(t.TrackUrl.ToString())}: {"play".ToQuantity(t.PlayCount, TaylorBotFormats.BoldReadable)}"
                            );

                            var embed = _lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithTitle($"Top tracks | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                                .WithDescription(formattedTracks.CreateEmbedDescriptionWithMaxAmountOfLines());

                            if (isLegacyCommand)
                            {
                                embed.WithFooter("⭐ Type /lastfm tracks for an improved command experience!");
                            }

                            return new EmbedResult(embed.Build());
                        }
                        else
                        {
                            return _lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, period.Value);
                        }

                    default: throw new NotImplementedException();
                }
            }
        );
    }

    public class LastFmTracksSlashCommand : ISlashCommand<LastFmTracksSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("lastfm tracks");
        public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

        private readonly LastFmTracksCommand _lastFmTracksCommand;

        public LastFmTracksSlashCommand(LastFmTracksCommand lastFmTracksCommand)
        {
            _lastFmTracksCommand = lastFmTracksCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _lastFmTracksCommand.Tracks(
                    options.period,
                    options.user.User,
                    isLegacyCommand: false
                )
            );
        }
    }
}
