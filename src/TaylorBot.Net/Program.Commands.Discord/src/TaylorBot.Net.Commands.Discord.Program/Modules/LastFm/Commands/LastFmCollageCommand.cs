using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Numbers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands
{
    public class LastFmCollageCommand
    {
        public static readonly CommandMetadata Metadata = new("lastfm collage", "Last.fm 🎶", new[] { "lastfm c", "fm collage", "np collage", "fm c", "np collage" });

        private readonly ILastFmUsernameRepository _lastFmUsernameRepository;
        private readonly LastFmEmbedFactory _lastFmEmbedFactory;
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;

        public LastFmCollageCommand(ILastFmUsernameRepository lastFmUsernameRepository, LastFmEmbedFactory lastFmEmbedFactory, LastFmPeriodStringMapper lastFmPeriodStringMapper)
        {
            _lastFmUsernameRepository = lastFmUsernameRepository;
            _lastFmEmbedFactory = lastFmEmbedFactory;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
        }

        public Command Collage(LastFmPeriod? period, LastFmCollageSize? size, IUser user, bool isLegacyCommand) => new(
            Metadata,
            async () =>
            {
                if (period == null)
                    period = LastFmPeriod.SevenDay;

                if (size == null)
                    size = new LastFmCollageSize(3);

                var lastFmUsername = await _lastFmUsernameRepository.GetLastFmUsernameAsync(user);

                if (lastFmUsername == null)
                    return _lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

                var queryString = new[] {
                    $"user={lastFmUsername.Username}",
                    $"period={_lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period.Value)}",
                    $"rows={size.Parsed}",
                    $"cols={size.Parsed}",
                    "imageSize=400",
                    $"a={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
                };

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithAuthor(
                        name: lastFmUsername.Username,
                        iconUrl: user.GetAvatarUrlOrDefault(),
                        url: lastFmUsername.LinkToProfile
                    )
                    .WithTitle($"Collage {size.Parsed}x{size.Parsed} | {_lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                    .WithImageUrl($"https://lastfmtopalbums.dinduks.com/patchwork.php?{string.Join('&', queryString)}");

                if (isLegacyCommand)
                {
                    embed.WithFooter(text: "⭐ Type /lastfm collage for an improved command experience!");
                }

                return new EmbedResult(embed.Build());
            }
        );
    }

    public class LastFmCollageSlashCommand : ISlashCommand<LastFmCollageSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("lastfm collage");
        public record Options(LastFmPeriod? period, ParsedOptionalInteger size, ParsedUserOrAuthor user);

        private readonly LastFmCollageCommand _lastFmCollageCommand;

        public LastFmCollageSlashCommand(LastFmCollageCommand lastFmCollageCommand)
        {
            _lastFmCollageCommand = lastFmCollageCommand;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(
                _lastFmCollageCommand.Collage(
                    options.period,
                    options.size.Value.HasValue ? new(options.size.Value.Value) : null,
                    options.user.User,
                    isLegacyCommand: false
                )
            );
        }
    }
}
