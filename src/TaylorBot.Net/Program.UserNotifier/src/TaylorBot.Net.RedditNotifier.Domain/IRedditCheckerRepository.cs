namespace TaylorBot.Net.RedditNotifier.Domain;

public interface IRedditCheckerRepository
{
    ValueTask<IReadOnlyCollection<RedditChecker>> GetRedditCheckersAsync();
    ValueTask UpdateLastPostAsync(RedditChecker redditChecker, RedditPost redditPost);
}
