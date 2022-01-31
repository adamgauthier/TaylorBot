using Reddit.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.RedditNotifier.Domain
{
    public interface IRedditCheckerRepository
    {
        ValueTask<IReadOnlyCollection<RedditChecker>> GetRedditCheckersAsync();
        ValueTask UpdateLastPostAsync(RedditChecker redditChecker, Post redditPost);
    }
}
