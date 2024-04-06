using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmCurrentCommand(IOptionsMonitor<LastFmOptions> options, LastFmEmbedFactory lastFmEmbedFactory, ILastFmUsernameRepository lastFmUsernameRepository, ILastFmClient lastFmClient)
{
    public static readonly CommandMetadata Metadata = new("lastfm current", "Last.fm 🎶", ["fm", "np"]);

    public Command Current(DiscordUser user) => new(
        Metadata,
        async () =>
        {
            var lastFmUsername = await lastFmUsernameRepository.GetLastFmUsernameAsync(user);

            if (lastFmUsername == null)
                return lastFmEmbedFactory.CreateLastFmNotSetEmbedResult(user);

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

                        return new EmbedResult(embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .AddField("Artist", mostRecentTrack.Artist.Name.DiscordMdLink(mostRecentTrack.Artist.Url), inline: true)
                            .AddField("Track", mostRecentTrack.TrackName.DiscordMdLink(mostRecentTrack.TrackUrl), inline: true)
                            .WithFooter(text: string.Join(" | ", [
                                mostRecentTrack.IsNowPlaying ? "Now Playing" : "Most Recent Track",
                                $"Total Scrobbles: {success.TotalScrobbles}"
                            ]), iconUrl: options.CurrentValue.LastFmEmbedFooterIconUrl)
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
                        Last.fm says your recent tracks are not public. 😢
                        Make sure 'Hide recent listening information' is off in your {"Last.fm privacy settings".DiscordMdLink("https://www.last.fm/settings/privacy")}!
                        """));

                case LastFmGenericErrorResult errorResult:
                    return lastFmEmbedFactory.CreateLastFmErrorEmbedResult(errorResult);

                default: throw new NotImplementedException();
            }
        }
    );
}

public class LastFmCurrentSlashCommand(LastFmCurrentCommand lastFmCurrentCommand) : ISlashCommand<LastFmCurrentSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("lastfm current");

    public record Options(ParsedFetchedUserOrAuthor user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(
            lastFmCurrentCommand.Current(new(options.user.User))
        );
    }
}
