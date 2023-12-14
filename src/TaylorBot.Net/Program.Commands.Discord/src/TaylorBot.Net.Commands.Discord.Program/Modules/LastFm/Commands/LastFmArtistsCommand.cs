using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmArtistsCommand(LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient, LastFmPeriodStringMapper lastFmPeriodStringMapper)
{
    public static readonly CommandMetadata Metadata = new("lastfm artists", "Last.fm 🎶", new[] { "fm artists", "np artists" });

    public Command Artists(LastFmPeriod? period, IUser user, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            if (period == null)
                period = LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

            var result = await lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

                case TopArtistsResult success:
                    if (success.TopArtists.Count > 0)
                    {
                        var formattedArtists = success.TopArtists.Select((a, index) =>
                            $"{index + 1}. {a.Name.DiscordMdLink(a.ArtistUrl.ToString())}: {"play".ToQuantity(a.PlayCount, TaylorBotFormats.BoldReadable)}"
                        );

                        var embed = lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle($"Top artists | {lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                            .WithDescription(formattedArtists.CreateEmbedDescriptionWithMaxAmountOfLines());

                        if (isLegacyCommand)
                        {
                            embed.WithFooter("⭐ Type /lastfm artists for an improved command experience!");
                        }

                        return new EmbedResult(embed.Build());
                    }
                    else
                    {
                        return lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, period.Value);
                    }

                default: throw new NotImplementedException();
            }
        }
    );
}

public class LastFmArtistsSlashCommand(LastFmArtistsCommand lastFmArtistsCommand) : ISlashCommand<LastFmArtistsSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm artists");
    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            lastFmArtistsCommand.Artists(
                options.period,
                options.user.User,
                isLegacyCommand: false
            )
        );
    }
}
