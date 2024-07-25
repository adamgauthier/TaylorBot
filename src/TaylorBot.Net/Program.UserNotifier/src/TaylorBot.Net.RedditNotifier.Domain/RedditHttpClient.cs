using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.RedditNotifier.Domain;

public record RedditPost(
    string id,
    float created_utc,
    string title,
    string author,
    string subreddit,
    string subreddit_name_prefixed,
    bool is_self,
    bool spoiler,
    uint num_comments,
    int score,
    string selftext,
    string thumbnail,
    string domain,
    string url)
{
    public DateTimeOffset CreatedAt => DateTimeOffset.FromUnixTimeSeconds((int)created_utc);
}

public record RedditChild(RedditPost data);

public record RedditData(IReadOnlyList<RedditChild> children);

public record RedditListing(RedditData data);

public class RedditHttpClient(ILogger<RedditHttpClient> logger, HttpClient httpClient, RedditTokenInMemoryRepository redditTokenRepository)
{
    public async Task<RedditPost> GetNewestPostAsync(string subreddit)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new($"https://oauth.reddit.com/r/{subreddit}/new.json?limit=1"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await redditTokenRepository.GetValidTokenAsync());

        var response = await httpClient.SendAsync(request);
        await response.EnsureSuccessAsync(logger);

        var responseAsString = await response.Content.ReadAsStringAsync();

        var redditListing = JsonSerializer.Deserialize<RedditListing>(responseAsString);
        ArgumentNullException.ThrowIfNull(redditListing);

        return redditListing.data.children.Single().data;
    }
}
