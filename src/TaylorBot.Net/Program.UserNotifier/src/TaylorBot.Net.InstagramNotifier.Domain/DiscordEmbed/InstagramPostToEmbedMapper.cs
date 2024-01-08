using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.InstagramNotifier.Domain.Options;

namespace TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;

public class InstagramPostToEmbedMapper(IOptionsMonitor<InstagramNotifierOptions> optionsMonitor)
{
    public Embed ToEmbed(InstagramPost post)
    {
        var options = optionsMonitor.CurrentValue;

        return new EmbedBuilder()
            .WithTitle(post.Caption != null ? post.Caption.Truncate(65) : "[No Caption]")
            .WithDescription($"`{post.LikesCount}` likes ❤, `{post.CommentsCount}` comments 💬")
            .WithThumbnailUrl(post.ThumbnailSrc)
            .WithUrl($"https://www.instagram.com/p/{post.ShortCode}/")
            .WithTimestamp(post.TakenAt)
            .WithAuthor(
                name: !string.IsNullOrWhiteSpace(post.AuthorFullName) ? post.AuthorFullName : post.AuthorUsername,
                url: $"https://www.instagram.com/{post.AuthorUsername}",
                iconUrl: post.AuthorProfilePicUrl
            )
            .WithFooter(text: "Instagram", iconUrl: options.InstagramPostEmbedIconUrl)
            .WithColor(DiscordColor.FromHexString(options.InstagramPostEmbedColor))
            .Build();
    }
}
