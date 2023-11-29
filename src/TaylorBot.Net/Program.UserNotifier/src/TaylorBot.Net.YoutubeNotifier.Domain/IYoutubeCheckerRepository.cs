using Google.Apis.YouTube.v3.Data;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public interface IYoutubeCheckerRepository
    {
        ValueTask<IReadOnlyCollection<YoutubeChecker>> GetYoutubeCheckersAsync();
        ValueTask UpdateLastPostAsync(YoutubeChecker youtubeChecker, PlaylistItemSnippet youtubePost);
    }
}
