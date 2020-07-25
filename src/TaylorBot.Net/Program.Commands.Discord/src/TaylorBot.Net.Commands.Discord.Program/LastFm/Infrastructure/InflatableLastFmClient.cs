using IF.Lastfm.Core.Api;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.Infrastructure
{
    public class InflatableLastFmClient : ILastFmClient
    {
        private readonly LastfmClient _client;

        public InflatableLastFmClient(LastfmClient client)
        {
            _client = client;
        }

        public async ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername)
        {
            var response = await _client.User.GetRecentScrobbles(
                username: lastFmUsername,
                count: 1
            );

            if (response.Success)
            {
                var content = response.Content.FirstOrDefault();

                return new MostRecentScrobbleResult(
                    totalScrobbles: response.TotalItems,
                    mostRecentTrack: content == null ? null : new MostRecentScrobble(
                        trackName: content.Name,
                        trackUrl: content.Url.ToString(),
                        trackImageUrl: content.Images.Large?.ToString(),
                        artist: new ScrobbleArtist(name: content.ArtistName, url: content.ArtistUrl.ToString()),
                        isNowPlaying: content.IsNowPlaying == true
                    )
                );
            }
            else
            {
                return new LastFmErrorResult(response.Status.ToString());
            }
        }
    }
}
