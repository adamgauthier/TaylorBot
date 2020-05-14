using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public interface IYoutubeCheckerRepository
    {
        Task<IEnumerable<YoutubeChecker>> GetYoutubeCheckersAsync();
        Task UpdateLastPostAsync(YoutubeChecker youtubeChecker, PlaylistItemSnippet youtubePost);
    }
}
