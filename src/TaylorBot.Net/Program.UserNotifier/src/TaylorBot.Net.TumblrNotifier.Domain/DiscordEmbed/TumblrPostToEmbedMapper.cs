using Discord;
using DontPanic.TumblrSharp.Client;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.TumblrNotifier.Domain.Options;

namespace TaylorBot.Net.TumblrNotifier.Domain.DiscordEmbed;

public class TumblrPostToEmbedMapper(IOptionsMonitor<TumblrNotifierOptions> optionsMonitor)
{
    public Embed ToEmbed(BasePost post, BlogInfo blog)
    {
        var options = optionsMonitor.CurrentValue;

        var builder = new EmbedBuilder()
            .WithTitle(string.IsNullOrWhiteSpace(post.Summary) ? "(no title)" : post.Summary.Replace("\n", " ").Truncate(65))
            .WithUrl(post.ShortUrl)
            .WithTimestamp(post.Timestamp)
            .WithAuthor(name: blog.Title, url: blog.Url)
            .WithFooter(text: "Tumblr", iconUrl: options.TumblrPostEmbedIconUrl)
            .WithColor(DiscordColor.FromHexString(options.TumblrPostEmbedColor));

        switch (post)
        {
            case LinkPost linkPost:
                if (!string.IsNullOrWhiteSpace(linkPost.Description))
                    builder.WithDescription(linkPost.Description.Truncate(EmbedBuilder.MaxDescriptionLength));
                builder.WithThumbnailUrl(options.TumblrLinkPostThumbnailUrl);
                break;

            case VideoPost videoPost:
                builder
                    .WithDescription(videoPost.VideoUrl.Truncate(EmbedBuilder.MaxDescriptionLength))
                    .WithThumbnailUrl(string.IsNullOrWhiteSpace(videoPost.ThumbnailUrl) ? options.TumblrDefaultVideoThumbnailUrl : videoPost.ThumbnailUrl);
                break;

            case PhotoPost photoPost:
                if (!string.IsNullOrWhiteSpace(photoPost.Caption))
                    builder.WithDescription(photoPost.Caption.Truncate(EmbedBuilder.MaxDescriptionLength));
                else if (photoPost.Tags.Any())
                    builder.WithDescription(string.Join(" ", photoPost.Tags.Select(t => $"#{t}")).Truncate(EmbedBuilder.MaxDescriptionLength));
                builder.WithThumbnailUrl(photoPost.Photo.OriginalSize.ImageUrl);
                break;

            case TextPost textPost:
                builder
                    .WithThumbnailUrl(options.TumblrTextPostThumbnailUrl)
                    .WithDescription(textPost.Body.Truncate(EmbedBuilder.MaxDescriptionLength));
                break;

            case ChatPost chatPost:
                builder.WithDescription(chatPost.Body.Truncate(EmbedBuilder.MaxDescriptionLength));
                break;

            case QuotePost quotePost:
                builder
                    .WithThumbnailUrl(options.TumblrQuotePostThumbnailUrl)
                    .WithDescription(quotePost.Text.Truncate(EmbedBuilder.MaxDescriptionLength));
                break;

            case AudioPost audioPost:
                builder.WithThumbnailUrl(options.TumblrAudioPostThumbnailUrl);
                break;

            case AnswerPost answerPost:
                builder.WithDescription(answerPost.Answer);
                break;
        }

        return builder.Build();
    }
}
