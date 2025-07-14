using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmAlbumsSlashCommand(
    LastFmEmbedFactory lastFmEmbedFactory,
    ILastFmUsernameRepository lastFmUsernameRepository,
    ILastFmClient lastFmClient,
    LastFmPeriodStringMapper lastFmPeriodStringMapper
) : ISlashCommand<LastFmAlbumsSlashCommand.Options>
{
    public static string CommandName => "lastfm albums";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm albums", ["fm albums", "np albums"]);

    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public Command Albums(LastFmPeriod? period, DiscordUser user, RunContext context) => new(
        context.SlashCommand == null ? Metadata with { IsSlashCommand = false } : Metadata,
        async () =>
        {
            period ??= LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
            {
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user, context);
            }

            var result = await lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case TopAlbumsResult success:
                    if (success.TopAlbums.Count > 0)
                    {
                        var formattedAlbums = success.TopAlbums.Select((a, index) =>
                            $"{index + 1}. {a.ArtistName.DiscordMdLink(a.ArtistUrl.ToString())} - {a.Name.DiscordMdLink(a.AlbumUrl.ToString())}: {"play".ToQuantity(a.PlayCount, TaylorBotFormats.BoldReadable)}"
                        );

                        var embed = lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle($"Top albums | {lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                            .WithDescription(formattedAlbums.CreateEmbedDescriptionWithMaxAmountOfLines());

                        var firstImageUrl = success.TopAlbums.Select(a => a.AlbumImageUrl).FirstOrDefault(url => url != null);
                        if (firstImageUrl != null)
                            embed.WithThumbnailUrl(firstImageUrl.ToString());

                        if (context.SlashCommand == null)
                        {
                            embed.WithFooter("⭐ Type /lastfm albums for an improved command experience!");
                        }

                        return new EmbedResult(embed.Build());
                    }
                    else
                    {
                        return lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, period.Value);
                    }

                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult, context);

                default: throw new NotImplementedException();
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            Albums(
                options.period,
                options.user.User,
                context
            )
        );
    }
}
