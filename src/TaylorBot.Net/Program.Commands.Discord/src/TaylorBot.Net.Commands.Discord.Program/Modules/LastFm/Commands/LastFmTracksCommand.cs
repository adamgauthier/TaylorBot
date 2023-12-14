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

public class LastFmTracksCommand(LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient, LastFmPeriodStringMapper lastFmPeriodStringMapper)
{
    public static readonly CommandMetadata Metadata = new("lastfm tracks", "Last.fm 🎶", new[] { "fm tracks", "np tracks" });

    public Command Tracks(LastFmPeriod? period, IUser user, bool isLegacyCommand) => new(
        Metadata,
        async () =>
        {
            if (period == null)
                period = LastFmPeriod.SevenDay;

            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

            var result = await lastFmClient.GetTopTracksAsync(lastFmUsername.Username, period.Value);

            switch (result)
            {
                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

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

                        if (isLegacyCommand)
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
}

public class LastFmTracksSlashCommand(LastFmTracksCommand lastFmTracksCommand) : ISlashCommand<LastFmTracksSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm tracks");
    public record Options(LastFmPeriod? period, ParsedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            lastFmTracksCommand.Tracks(
                options.period,
                options.user.User,
                isLegacyCommand: false
            )
        );
    }
}
