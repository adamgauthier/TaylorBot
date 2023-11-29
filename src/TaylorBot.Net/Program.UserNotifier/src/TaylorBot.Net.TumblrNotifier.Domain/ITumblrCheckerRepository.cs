using DontPanic.TumblrSharp.Client;

namespace TaylorBot.Net.TumblrNotifier.Domain
{
    public interface ITumblrCheckerRepository
    {
        ValueTask<IReadOnlyCollection<TumblrChecker>> GetTumblrCheckersAsync();
        ValueTask UpdateLastPostAsync(TumblrChecker tumblrChecker, BasePost tumblrPost);
    }
}
