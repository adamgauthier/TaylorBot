using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;

public class LastFmEmbedFactory(LastFmPeriodStringMapper lastFmPeriodStringMapper, CommandMentioner mention)
{
    public EmbedResult CreateLastFmNoScrobbleErrorEmbedResult(LastFmUsername lastFmUsername, DiscordUser user, LastFmPeriod period)
    {
        return new(CreateBaseLastFmEmbed(lastFmUsername, user)
            .WithColor(TaylorBotColors.ErrorColor)
            .WithDescription(
                $"""
                This Last.fm account doesn't have scrobbles for period '{lastFmPeriodStringMapper.MapLastFmPeriodToReadableString(period)}' 🔍
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

    public EmbedResult CreateLastFmNotSetEmbedResult(DiscordUser user, RunContext context)
    {
        return new(EmbedFactory.CreateError(
            $"""
            {user.Mention}'s Last.fm username is not set 🚫
            Last.fm can track your listening habits on any platform. You can create a Last.fm account by {"clicking here".DiscordMdLink("https://www.last.fm/join")}.
            You can then link it to TaylorBot with {mention.SlashCommand("lastfm set", context)}.
            """
        ));
    }

    private EmbedResult CreateLastFmNotFoundEmbedResult(LastFmUserNotFound notFound, RunContext context)
    {
        return new(EmbedFactory.CreateError(
            $"""
            This Last.fm account ({notFound.Username}) can't be found 🚫
            Make sure the username set with {mention.SlashCommand("lastfm set", context)} is an existing Last.fm account 👆
            """
        ));
    }

    public EmbedResult CreateLastFmErrorEmbedResult(LastFmGenericErrorResult error, RunContext context)
    {
        if (error is LastFmUserNotFound notFound)
        {
            return CreateLastFmNotFoundEmbedResult(notFound, context);
        }

        return new(EmbedFactory.CreateError(
            $"""
            Last.fm returned an error. {(error.Error != null ? $"({error.Error}) " : string.Empty)}😢
            The site might be down. Try again later!
            """));
    }
}
