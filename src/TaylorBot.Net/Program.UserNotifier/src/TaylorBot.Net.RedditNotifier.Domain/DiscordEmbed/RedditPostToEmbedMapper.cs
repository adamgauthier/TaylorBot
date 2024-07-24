using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.RedditNotifier.Domain.Options;

namespace TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed;

public class RedditPostToEmbedMapper(IOptionsMonitor<RedditNotifierOptions> optionsMonitor)
{
    private static readonly string[] DOMAINS_TO_USE_URL_AS_THUMBNAIL = ["i.redd.it", "i.imgur.com"];

    public Embed ToEmbed(string subreddit, RedditPost post)
    {
        var options = optionsMonitor.CurrentValue;

        var builder = new EmbedBuilder()
            .WithTitle(post.title.Truncate(65))
            .WithUrl($"https://redd.it/{post.id}")
            .WithTimestamp(post.CreatedAt)
            .WithAuthor(name: $"r/{subreddit}", url: $"https://www.reddit.com/r/{subreddit}")
            .WithFooter(text: $"u/{post.author}", iconUrl: options.RedditPostEmbedIconUrl)
            .WithColor(DiscordColor.FromHexString(options.RedditPostEmbedColor));

        if (post.is_self)
        {
            builder.WithDescription(post.spoiler ?
                options.RedditPostEmbedSelfPostSpoilerDescription : post.selftext.Truncate(400)
            );
        }
        else
        {
            builder
                .WithDescription($"🔺 {"point".ToQuantity(post.score, TaylorBotFormats.CodedReadable)}, {"comment".ToQuantity(post.num_comments, TaylorBotFormats.CodedReadable)} 💬")
                .WithThumbnailUrl(post.spoiler ? options.RedditPostEmbedLinkPostSpoilerThumbnailUrl :
                    DOMAINS_TO_USE_URL_AS_THUMBNAIL.Any(domain => domain == post.domain) ? post.url :
                        post.thumbnail is "default" or "nsfw" ? options.RedditPostEmbedLinkPostNoThumbnailUrl : post.thumbnail
                );
        }

        return builder.Build();
    }
}
