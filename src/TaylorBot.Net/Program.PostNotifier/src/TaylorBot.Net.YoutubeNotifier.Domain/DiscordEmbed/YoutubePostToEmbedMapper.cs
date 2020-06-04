using Discord;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;

namespace TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed
{
    public class YoutubePostToEmbedMapper
    {
        private readonly IOptionsMonitor<YoutubeNotifierOptions> _optionsMonitor;

        public YoutubePostToEmbedMapper(IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public Embed ToEmbed(ParsedPlaylistItemSnippet parsedSnippet)
        {
            var options = _optionsMonitor.CurrentValue;
            var post = parsedSnippet.Snippet;

            var builder = new EmbedBuilder()
                .WithTitle(post.Title.Truncate(65))
                .WithDescription(post.Description.Truncate(200))
                .WithUrl($"https://youtu.be/{post.ResourceId.VideoId}")
                .WithThumbnailUrl(post.Thumbnails.Medium.Url)
                .WithAuthor(name: post.ChannelTitle, url: $"https://www.youtube.com/channel/{post.ChannelId}")
                .WithFooter(text: "YouTube", iconUrl: options.YoutubePostEmbedIconUrl)
                .WithColor(DiscordColor.FromHexString(options.YoutubePostEmbedColor));

            if (parsedSnippet.PublishedAt.HasValue)
                builder.WithTimestamp(parsedSnippet.PublishedAt.Value);

            return builder.Build();
        }
    }
}
