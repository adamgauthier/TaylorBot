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
    public class LastFmAlbumsCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm albums", "Last.fm 🎶", new[] { "fm albums", "np albums" });

        private readonly LastFmEmbedFactory _lastFmEmbedFactory;
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly ILastFmClient _lastFmClient;
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;

        public LastFmAlbumsCommand(LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient, LastFmPeriodStringMapper lastFmPeriodStringMapper)
        {
            _lastFmEmbedFactory = lastFmEmbedFactory;
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmClient = lastFmClient;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
        }

        public Command Albums(LastFmPeriod? period, IUser user, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                if (period == null)
                    period = LastFmPeriod.SevenDay;

                var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                if (lastFmUsername == null)
                    return _lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                var result = await _lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period.Value);

                switch (result)
                {
                    case LastFmGenericErrorResult errorResult:
                        return _lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

                    case TopAlbumsResult success:
                        if (success.TopAlbums.Count > 0)
                        {
                            var formattedAlbums = success.TopAlbums.Select((a, index) =>
                                $"{index + 1}. {a.ArtistName.DiscordMdLink(a.ArtistUrl.ToString())} - {a.Name.DiscordMdLink(a.AlbumUrl.ToString())}: {"play".ToQuantity(a.PlayCount, TaylorBotFormats.BoldReadable)}"
                            );

                            var embed = _lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                                .WithColor(TaylorBotColors.SuccessColor)
                                .WithTitle($"Top albums | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                                .WithDescription(formattedAlbums.CreateEmbedDescriptionWithMaxAmountOfLines());

                            var firstImageUrl = success.TopAlbums.Select(a => a.AlbumImageUrl).FirstOrDefault(url => url != null);
                            if (firstImageUrl != null)
                                embed.WithThumbnailUrl(firstImageUrl.ToString());

                            if (isLegacyCommand)
                            {
                                embed.WithFooter("⭐ Type /lastfm albums for an improved command experience!");
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

    public class LastFmAlbumsSlashCommand : ISlashCommand<LastFmAlbumsSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("lastfm albums");
        public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

        private readonly LastFmAlbumsCommand _lastFmAlbumsCommand;

        public LastFmAlbumsSlashCommand(LastFmAlbumsCommand lastFmAlbumsCommand)
        {
            _lastFmAlbumsCommand = lastFmAlbumsCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _lastFmAlbumsCommand.Albums(
                    options.period,
                    options.user.User,
                    isLegacyCommand: false
                )
            );
        }
    }
}
