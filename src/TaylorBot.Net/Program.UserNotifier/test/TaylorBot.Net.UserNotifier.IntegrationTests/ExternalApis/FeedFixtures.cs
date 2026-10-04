using TaylorBot.Net.IntegrationTests.Shared.ExternalApis;
using TaylorBot.Net.IntegrationTests.Shared.Hosting;
using TaylorBot.Net.RedditNotifier.Domain;
using TaylorBot.Net.TumblrNotifier.Domain;
using TaylorBot.Net.UserNotifier.Program.Jobs;
using TaylorBot.Net.YoutubeNotifier.Domain;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.ExternalApis;

public sealed class FeedFixtures(ExternalApi external)
{
    public const string RedditTokenUri = "https://www.reddit.com/api/v1/access_token";
    public const string RedditListingUri = "https://oauth.reddit.com/r/integration/new.json?limit=1";
    public const string YoutubeListingUri = "https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=integration&key=synthetic";
    public const string TumblrListingUri = "https://api.tumblr.com/v2/blog/integration.tumblr.com/posts?api_key=synthetic&filter=text&limit=1";

    public void Posts(UserNotifierJob job, bool empty = false)
    {
        switch (job)
        {
            case UserNotifierJob.Reddit:
                Reddit(empty);
                break;
            case UserNotifierJob.Youtube:
                Youtube(empty);
                break;
            case UserNotifierJob.Tumblr:
                Tumblr(empty);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(job));
        }
    }

    public static void ExpectCheckerFailure(ScenarioLogs logs, UserNotifierJob job)
    {
        const string Message = "Exception occurred when checking";
        switch (job)
        {
            case UserNotifierJob.Reddit:
                logs.ExpectError<RedditNotifierService>(Message);
                break;
            case UserNotifierJob.Youtube:
                logs.ExpectError<YoutubeNotifierService>(Message);
                break;
            case UserNotifierJob.Tumblr:
                logs.ExpectError<TumblrNotifierService>(Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(job));
        }
    }

    public void MalformedPosts(UserNotifierJob job)
    {
        var uri = job switch
        {
            UserNotifierJob.Reddit => RedditListingUri,
            UserNotifierJob.Youtube => YoutubeListingUri,
            UserNotifierJob.Tumblr => TumblrListingUri,
            _ => throw new ArgumentOutOfRangeException(nameof(job)),
        };

        if (job == UserNotifierJob.Reddit)
        {
            RedditToken();
        }

        external.Raw("GET", uri, "{", "application/json");
    }

    private void RedditToken() =>
        external.Json("POST", RedditTokenUri, new { access_token = "synthetic-access-token", token_type = "bearer", expires_in = 3600, scope = "read" });

    private void Reddit(bool empty)
    {
        RedditToken();

        var post = new
        {
            kind = "t3",
            data = new
            {
                id = "post123",
                created_utc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                title = "Integration post",
                author = "SyntheticAuthor",
                subreddit = "integration",
                subreddit_name_prefixed = "r/integration",
                is_self = true,
                spoiler = false,
                num_comments = 0,
                score = 1,
                selftext = "Fresh notification content",
                thumbnail = "self",
                domain = "self.integration",
                url = "https://www.reddit.com/r/integration/comments/post123",
            },
        };

        external.Json("GET", RedditListingUri, new { kind = "Listing", data = new { children = empty ? [] : new[] { post } } });
    }

    private void Youtube(bool empty)
    {
        var post = new
        {
            kind = "youtube#playlistItem",
            id = "item123",
            snippet = new
            {
                publishedAt = "2026-01-01T00:00:00Z",
                title = "Integration post",
                description = "Fresh notification content",
                channelTitle = "Integration channel",
                channelId = "channel123",
                thumbnails = new { medium = new { url = "https://images.invalid/video.png", width = 320, height = 180 } },
                resourceId = new { kind = "youtube#video", videoId = "video123" },
            },
        };

        external.Json("GET", YoutubeListingUri, new { kind = "youtube#playlistItemListResponse", items = empty ? [] : new[] { post } });
    }

    private void Tumblr(bool empty)
    {
        var post = new
        {
            id = 123,
            blog_name = "integration",
            type = "text",
            state = "published",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            date = "2026-01-01 00:00:00 GMT",
            post_url = "https://integration.tumblr.com/post/123",
            short_url = "https://tmblr.co/post123",
            summary = "Integration post",
            title = "Integration post",
            body = "Fresh notification content",
            tags = Array.Empty<string>(),
            format = "html",
            note_count = 0,
        };

        external.Json("GET", TumblrListingUri, new
        {
            meta = new { status = 200, msg = "OK" },
            response = new
            {
                blog = new { name = "integration", title = "Integration blog", url = "https://integration.tumblr.com/", updated = post.timestamp, posts = 1 },
                posts = empty ? [] : new[] { post },
                total_posts = empty ? 0 : 1,
            },
        });
    }
}
