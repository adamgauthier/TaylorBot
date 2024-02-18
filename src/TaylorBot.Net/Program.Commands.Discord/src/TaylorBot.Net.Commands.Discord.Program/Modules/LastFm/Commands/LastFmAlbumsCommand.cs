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

public class LastFmAlbumsCommand(LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient, LastFmPeriodStringMapper lastFmPeriodStringMapper)
{
    public static readonly CommandMetadata Metadata = new("lastfm albums", "Last.fm 🎶", ["fm albums", "np albums"]);

    public Command Albums(LastFmPeriod? period, IUser user, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            period ??= LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

            var result = await lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

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

                        if (isLegacyCommand)
                        {
                            embed.WithFooter("⭐ Type /lastfm albums for an improved command experience!");
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

public class LastFmAlbumsSlashCommand(LastFmAlbumsCommand lastFmAlbumsCommand) : ISlashCommand<LastFmAlbumsSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm albums");
    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            lastFmAlbumsCommand.Albums(
                options.period,
                options.user.User,
                isLegacyCommand: false
            )
        );
    }
}
