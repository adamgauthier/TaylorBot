using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using Reddit.Controllers;
using System.Linq;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.RedditNotifier.Domain.Options;

namespace TaylorBot.Net.RedditNotifier.Domain.DiscordEmbed
{
    public class RedditPostToEmbedMapper
    {
        private static readonly string[] DOMAINS_TO_USE_URL_AS_THUMBNAIL = new[] { "i.redd.it", "i.imgur.com" };

        private IOptionsMonitor<RedditNotifierOptions> optionsMonitor;

        public RedditPostToEmbedMapper(IOptionsMonitor<RedditNotifierOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        public Embed ToEmbed(Post post)
        {
            var options = optionsMonitor.CurrentValue;

            var builder = new EmbedBuilder()
                .WithTitle(post.Title.Truncate(65))
                .WithUrl($"https://redd.it/{post.Id}")
                .WithTimestamp(post.Created)
                .WithAuthor(name: $"r/{post.Subreddit}", url: $"https://www.reddit.com/r/{post.Subreddit}")
                .WithFooter(text: $"u/{post.Author}", iconUrl: options.RedditPostEmbedIconUrl)
                .WithColor(DiscordColor.FromHexString(options.RedditPostEmbedColor));

            switch (post)
            {
                case SelfPost selfPost:
                    builder.WithDescription(selfPost.Listing.Spoiler ?
                        options.RedditPostEmbedSelfPostSpoilerDescription : selfPost.SelfText.Truncate(400)
                    );
                    break;
                case LinkPost linkPost:
                    builder
                        .WithDescription($"🔺 {"point".ToQuantity(post.Score, TaylorBotFormats.CodedReadable)}, {"comment".ToQuantity(post.Listing.NumComments, TaylorBotFormats.CodedReadable)} 💬")
                        .WithThumbnailUrl(post.Listing.Spoiler ? options.RedditPostEmbedLinkPostSpoilerThumbnailUrl :
                            DOMAINS_TO_USE_URL_AS_THUMBNAIL.Any(domain => domain == linkPost.Listing.Domain) ? linkPost.URL :
                                linkPost.Thumbnail is "default" or "nsfw" ? options.RedditPostEmbedLinkPostNoThumbnailUrl : linkPost.Thumbnail
                        );
                    break;
            }

            return builder.Build();
        }
    }
}
