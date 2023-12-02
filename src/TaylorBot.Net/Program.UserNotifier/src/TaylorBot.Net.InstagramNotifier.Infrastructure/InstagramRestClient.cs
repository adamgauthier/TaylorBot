using System.Text.Json;
using TaylorBot.Net.InstagramNotifier.Domain;

namespace TaylorBot.Net.InstagramNotifier.Infrastructure;

public class InstagramRestClient : IInstagramClient
{
    private readonly HttpClient _httpClient = new();

    public async ValueTask<InstagramPost> GetLatestPostAsync(string username)
    {
        var jsonDocument = await JsonDocument.ParseAsync(
            await _httpClient.GetStreamAsync($"https://www.instagram.com/{username}/?__a=1")
        );

        var user = jsonDocument.RootElement.GetProperty("graphql").GetProperty("user");

        if (user.GetProperty("is_private").GetBoolean())
            throw new InvalidOperationException("User is private.");

        var edge_owner_to_timeline_media = user.GetProperty("edge_owner_to_timeline_media");

        if (edge_owner_to_timeline_media.GetProperty("count").GetUInt64() == 0)
            throw new InvalidOperationException("Media list is empty.");

        var item = edge_owner_to_timeline_media.GetProperty("edges").EnumerateArray().First().GetProperty("node");
        var captionEdges = item.GetProperty("edge_media_to_caption").GetProperty("edges").EnumerateArray().ToList();

        return new InstagramPost(
            shortCode: item.GetProperty("shortcode").GetString()!,
            takenAt: DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("taken_at_timestamp").GetInt64()),
            likesCount: item.GetProperty("edge_liked_by").GetProperty("count").GetUInt64(),
            commentsCount: item.GetProperty("edge_media_to_comment").GetProperty("count").GetUInt64(),
            thumbnailSrc: item.GetProperty("thumbnail_src").GetString()!,
            caption: captionEdges.Any() ? captionEdges.First().GetProperty("node").GetProperty("text").GetString()! : null,
            authorFullName: user.GetProperty("full_name").GetString()!,
            authorUsername: user.GetProperty("username").GetString()!,
            authorProfilePicUrl: user.GetProperty("profile_pic_url").GetString()!
        );
    }
}
