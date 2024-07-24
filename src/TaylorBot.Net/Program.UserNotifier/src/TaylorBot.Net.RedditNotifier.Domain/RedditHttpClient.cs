using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TaylorBot.Net.RedditNotifier.Domain;

public record RedditPost(
    string id,
    float created_utc,
    string title,
    string author,
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

public class RedditHttpClient(ILogger<RedditHttpClient> logger, HttpClient httpClient)
{
    public async Task<RedditPost> GetNewestPostAsync(string subreddit)
    {
        var response = await httpClient.GetAsync($"https://www.reddit.com/r/{subreddit}/new.json?sort=new&limit=1");

        if (response.IsSuccessStatusCode)
        {
            var responseAsString = await response.Content.ReadAsStringAsync();

            var redditListing = JsonSerializer.Deserialize<RedditListing>(responseAsString);
            ArgumentNullException.ThrowIfNull(redditListing);

            return redditListing.data.children.Single().data;
        }
        else
        {
            logger.LogWarning("Unexpected status code when fetching from Reddit ({StatusCode}).", response.StatusCode);
            throw new InvalidOperationException();
        }
    }
}
