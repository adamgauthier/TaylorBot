using Discord;
using Google.Apis.YouTube.v3.Data;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.YoutubeNotifier.Domain.Options;

namespace TaylorBot.Net.YoutubeNotifier.Domain.DiscordEmbed;

public class YoutubePostToEmbedMapper(IOptionsMonitor<YoutubeNotifierOptions> optionsMonitor)
{
    public Embed ToEmbed(PlaylistItemSnippet post)
    {
        var options = optionsMonitor.CurrentValue;

        var builder = new EmbedBuilder()
            .WithTitle(post.Title.Truncate(65))
            .WithDescription(post.Description.Truncate(200))
            .WithUrl($"https://youtu.be/{post.ResourceId.VideoId}")
            .WithThumbnailUrl(post.Thumbnails.Medium.Url)
            .WithAuthor(name: post.ChannelTitle, url: $"https://www.youtube.com/channel/{post.ChannelId}")
            .WithFooter(text: "YouTube", iconUrl: options.YoutubePostEmbedIconUrl)
            .WithColor(DiscordColor.FromHexString(options.YoutubePostEmbedColor));

        if (post.PublishedAtDateTimeOffset.HasValue)
            builder.WithTimestamp(post.PublishedAtDateTimeOffset.Value);

        return builder.Build();
    }
}
