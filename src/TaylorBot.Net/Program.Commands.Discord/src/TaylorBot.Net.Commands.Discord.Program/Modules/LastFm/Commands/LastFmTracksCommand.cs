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

public class LastFmTracksSlashCommand(
    LastFmEmbedFactory lastFmEmbedFactory,
    ILastFmUsernameRepository lastFmUsernameRepository,
    ILastFmClient lastFmClient,
    LastFmPeriodStringMapper lastFmPeriodStringMapper
) : ISlashCommand<LastFmTracksSlashCommand.Options>
{
    public static string CommandName => "lastfm tracks";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm tracks", ["fm tracks", "np tracks"]);

    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public Command Tracks(LastFmPeriod? period, DiscordUser user, RunContext context) => new(
        context.SlashCommand == null ? Metadata with { IsSlashCommand = false } : Metadata,
        async () =>
        {
            period ??= LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user, context);

            var result = await lastFmClient.GetTopTracksAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult, context);

                case TopTracksResult success:
                    if (success.TopTracks.Count > 0)
                    {
                        var formattedTracks = success.TopTracks.Select((t, index) =>
                            $"{index + 1}. {t.ArtistName.DiscordMdLink(t.ArtistUrl.ToString())} - {t.Name.DiscordMdLink(t.TrackUrl.ToString())}: {"play".ToQuantity(t.PlayCount, TaylorBotFormats.BoldReadable)}"
                        );

                        var embed = lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user)
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithTitle($"Top tracks | {lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period.Value)}")
                            .WithDescription(formattedTracks.CreateEmbedDescriptionWithMaxAmountOfLines());

                        if (context.SlashCommand == null)
                        {
                            embed.WithFooter("⭐ Type /lastfm tracks for an improved command experience!");
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

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            Tracks(
                options.period,
                options.user.User,
                context
            )
        );
    }
}
