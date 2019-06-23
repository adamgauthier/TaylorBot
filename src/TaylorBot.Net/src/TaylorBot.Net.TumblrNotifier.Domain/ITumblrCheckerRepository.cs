using DontPanic.TumblrSharp.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public interface ITumblrCheckerRepository
    {
        Task<IEnumerable<TumblrChecker>> GetTumblrCheckersAsync();
        Task UpdateLastPostAsync(TumblrChecker tumblrChecker, BasePost tumblrPost);
    }
}
