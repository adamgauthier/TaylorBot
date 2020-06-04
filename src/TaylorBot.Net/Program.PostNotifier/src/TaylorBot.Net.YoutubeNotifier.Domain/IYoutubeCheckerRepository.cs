using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public interface IYoutubeCheckerRepository
    {
        ValueTask<IReadOnlyCollection<YoutubeChecker>> GetYoutubeCheckersAsync();
        ValueTask UpdateLastPostAsync(YoutubeChecker youtubeChecker, ParsedPlaylistItemSnippet youtubePost);
    }
}
