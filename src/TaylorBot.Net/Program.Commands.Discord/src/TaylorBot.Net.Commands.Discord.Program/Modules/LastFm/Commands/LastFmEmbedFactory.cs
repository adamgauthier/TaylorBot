using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmEmbedFactory(LastFmPeriodStringMapper lastFmPeriodStringMapper)
{
    public EmbedResult CreateLastFmNoScrobbleErrorEmbedResult(LastFmUsername lastFmUsername, DiscordUser user, LastFmPeriod period)
    {
        return new(CreateBaseLastFmEmbed(lastFmUsername, user)
            .WithColor(TaylorBotColors.ErrorColor)
            .WithDescription(
                $"""
                This account does not have any scrobbles for period '{lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}'. 🔍
                Start listening to a song and scrobble it to Last.fm so it shows up here!
                """)
        .Build());
    }

    public EmbedBuilder CreateBaseLastFmEmbed(LastFmUsername lastFmUsername, DiscordUser user)
    {
        return new EmbedBuilder().WithAuthor(
            name: lastFmUsername.Username,
            iconUrl: user.GetGuildAvatarUrlOrDefault(),
            url: lastFmUsername.LinkToProfile
        );
    }

    public EmbedResult CreateLastFmNotSetEmbedResult(DiscordUser user)
    {
        return new(EmbedFactory.CreateError(
            $"""
            {user.Mention}'s Last.fm username is not set. 🚫",
            Last.fm can track your listening habits on any platform. You can create a Last.fm account by {"clicking here".DiscordMdLink("https://www.last.fm/join")}.
            You can then link it to TaylorBot with </lastfm set:922354806574678086>.
            """
        ));
    }

    public EmbedResult CreateLastFmErrorEmbedResult(LastFmGenericErrorResult error)
    {
        return new(EmbedFactory.CreateError(
            $"""
            Last.fm returned an error. {(error.Error != null ? $"({error.Error}) " : string.Empty)}😢
            The site might be down. Try again later!
            """));
    }
}
