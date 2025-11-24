using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public partial class LastFmCurrentSlashCommand(
    ILogger<LastFmCurrentSlashCommand> logger,
    IOptionsMonitor<LastFmOptions> options,
    LastFmEmbedFactory lastFmEmbedFactory,
    ILastFmUsernameRepository lastFmUsernameRepository,
    ILastFmClient lastFmClient
) : ISlashCommand<LastFmCurrentSlashCommand.Options>
{
    public static string CommandName => "lastfm current";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public static readonly CommandMetadata Metadata = new("lastfm current", ["fm", "np"]);

    public record Options(ParsedUserOrAuthor user);

    public Command Current(DiscordUser user, RunContext context) => new(
        context.SlashCommand != null ? Metadata : Metadata with { IsSlashCommand = false },
        async () =>
        {
            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user, context);

            var result = await lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username);

            switch (result)
            {
                case MostRecentScrobbleResult success:
                    if (success.MostRecentTrack != null)
                    {
                        var embed = lastFmEmbedFactory.CreateBaseLastFmEmbed(lastFmUsername, user);

                        var mostRecentTrack = success.MostRecentTrack;
                        if (mostRecentTrack.TrackImageUrl != null)
                        {
                            embed.WithThumbnailUrl(mostRecentTrack.TrackImageUrl);
                        }

                        var footer = mostRecentTrack.IsNowPlaying ? "Now Playing" : "Last Played";

                        var userTrackPlayCount = await TryGetTrackPlayCountAsync(lastFmUsername, mostRecentTrack);
                        if (userTrackPlayCount != null)
                        {
                            footer += mostRecentTrack.IsNowPlaying && userTrackPlayCount.Value == 0
                                ? $" | First time playing this song!"
                                : $" | Song played {"time".ToQuantity(userTrackPlayCount.Value, TaylorBotFormats.Readable, TaylorBotCulture.Culture)} total";
                        }

                        return new EmbedResult(embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .AddField("Artist", mostRecentTrack.Artist.Name.DiscordMdLink(mostRecentTrack.Artist.Url), inline: true)
                            .AddField("Track", mostRecentTrack.TrackName.DiscordMdLink(mostRecentTrack.TrackUrl), inline: true)
                            .WithFooter(text: footer, iconUrl: options.CurrentValue.LastFmEmbedFooterIconUrl)
                            .Build()
                        );
                    }
                    else
                    {
                        return lastFmEmbedFactory.CreateLastFmNoScrobbleErrorEmbedResult(lastFmUsername, user, LastFmPeriod.Overall);
                    }

                case LastFmLogInRequiredErrorResult _:
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        Last.fm says your recent tracks are not public 😢
                        Make sure 'Hide recent listening information' is off in your {"Last.fm privacy settings".DiscordMdLink("https://www.last.fm/settings/privacy")}!
                        """));

                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult, context);

                default: throw new NotImplementedException();
            }
        }
    );

    private async Task<int?> TryGetTrackPlayCountAsync(LastFmUsername lastFmUsername, MostRecentScrobble mostRecentTrack)
    {
        try
        {
            var trackInfo = await lastFmClient.GetTrackInfoAsync(lastFmUsername.Username, mostRecentTrack);
            return trackInfo?.UserPlayCount;
        }
        catch (Exception e)
        {
            LogFailedToFetchTrackPlayCount(e, lastFmUsername.Username, mostRecentTrack.Artist.Name, mostRecentTrack.TrackName);
            return null;
        }
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Current(options.user.User, context));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to fetch user track play count for {Username} - {Artist} - {Track}")]
    private partial void LogFailedToFetchTrackPlayCount(Exception exception, string username, string artist, string track);
}
