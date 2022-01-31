using DontPanic.TumblrSharp.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public interface ITumblrCheckerRepository
    {
        ValueTask<IReadOnlyCollection<TumblrChecker>> GetTumblrCheckersAsync();
        ValueTask UpdateLastPostAsync(TumblrChecker tumblrChecker, BasePost tumblrPost);
    }
}
