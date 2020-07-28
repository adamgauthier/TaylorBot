using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using System;
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

        private LastStatsTimeSpan MapPeriodToTimeSpan(LastFmPeriod period)
        {
            return period switch
            {
                LastFmPeriod.SevenDay => LastStatsTimeSpan.Week,
                LastFmPeriod.OneMonth => LastStatsTimeSpan.Month,
                LastFmPeriod.ThreeMonth => LastStatsTimeSpan.Quarter,
                LastFmPeriod.SixMonth => LastStatsTimeSpan.Half,
                LastFmPeriod.TwelveMonth => LastStatsTimeSpan.Year,
                LastFmPeriod.Overall => LastStatsTimeSpan.Overall,
                _ => throw new ArgumentOutOfRangeException(nameof(period)),
            };
        }

        public async ValueTask<ITopArtistsResultResult> GetTopArtistsAsync(string lastFmUsername, LastFmPeriod period)
        {
            var response = await _client.User.GetTopArtists(
                username: lastFmUsername,
                span: MapPeriodToTimeSpan(period),
                pagenumber: 1,
                count: 10
            );

            if (response.Success)
            {
                var content = response.Content;

                return new TopArtistsResult(response.Content.Select(a => new TopArtist(
                    name: a.Name,
                    artistUrl: a.Url,
                    playCount: a.PlayCount ?? 0
                )).ToList());
            }
            else
            {
                return new LastFmErrorResult(response.Status.ToString());
            }
        }
    }
}
