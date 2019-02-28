using Reddit.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.RedditNotifier.Domain
{
    public interface IRedditCheckerRepository
    {
        Task<IEnumerable<RedditChecker>> GetRedditCheckersAsync();
        Task UpdateLastPostAsync(RedditChecker redditChecker, Post redditPost);
    }
}
