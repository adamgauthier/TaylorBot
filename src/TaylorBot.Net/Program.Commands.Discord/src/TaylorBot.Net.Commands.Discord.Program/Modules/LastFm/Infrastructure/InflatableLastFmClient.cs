using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure
{
    public class InflatableLastFmClient : ILastFmClient
    {
        private readonly HttpClient _httpClient = new();

        private readonly ILogger<InflatableLastFmClient> _logger;
        private readonly IOptionsMonitor<LastFmOptions> _options;
        private readonly LastfmClient _client;
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper;

        public InflatableLastFmClient(ILogger<InflatableLastFmClient> logger, IOptionsMonitor<LastFmOptions> options, LastfmClient client, LastFmPeriodStringMapper lastFmPeriodStringMapper)
        {
            _logger = logger;
            _options = options;
            _client = client;
            _lastFmPeriodStringMapper = lastFmPeriodStringMapper;
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
                    TotalScrobbles: response.TotalItems,
                    MostRecentTrack: content == null ? null : new MostRecentScrobble(
                        TrackName: content.Name,
                        TrackUrl: content.Url.ToString(),
                        TrackImageUrl: content.Images.Large?.ToString(),
                        Artist: new ScrobbleArtist(Name: content.ArtistName, Url: content.ArtistUrl.ToString()),
                        IsNowPlaying: content.IsNowPlaying == true
                    )
                );
            }
            else if (response.Status == (LastResponseStatus)17)
            {
                return new LastFmLogInRequiredErrorResult();
            }
            else
            {
                return new LastFmGenericErrorResult(response.Status.ToString());
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

        public async ValueTask<ITopArtistsResult> GetTopArtistsAsync(string lastFmUsername, LastFmPeriod period)
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
                    Name: a.Name,
                    ArtistUrl: a.Url,
                    PlayCount: a.PlayCount ?? 0
                )).ToList());
            }
            else
            {
                return new LastFmGenericErrorResult(response.Status.ToString());
            }
        }

        public async ValueTask<ITopTracksResult> GetTopTracksAsync(string lastFmUsername, LastFmPeriod period)
        {
            var queryString = new[] {
                "method=user.gettoptracks",
                $"user={lastFmUsername}",
                $"api_key={_options.CurrentValue.LastFmApiKey}",
                $"period={_lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
                "format=json",
                "page=1",
                "limit=10"
            };

            var response = await _httpClient.GetAsync($"https://ws.audioscrobbler.com/2.0/?{string.Join('&', queryString)}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                var tracks = jsonDocument.RootElement.GetProperty("toptracks").GetProperty("track");

                return new TopTracksResult(tracks.EnumerateArray().Select(t => new TopTrack(
                    Name: t.GetProperty("name").GetString()!,
                    TrackUrl: new Uri(t.GetProperty("url").GetString()!),
                    PlayCount: int.Parse(t.GetProperty("playcount").GetString()!),
                    ArtistName: t.GetProperty("artist").GetProperty("name").GetString()!,
                    ArtistUrl: new Uri(t.GetProperty("artist").GetProperty("url").GetString()!)
                )).ToList());
            }
            else
            {
                try
                {
                    var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    var status = (LastResponseStatus)jsonDocument.RootElement.GetProperty("error").GetUInt16();
                    return new LastFmGenericErrorResult(status.ToString());
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Unhandled error when parsing json in Last.fm error response ({response.StatusCode}):");
                    return new LastFmGenericErrorResult(null);
                }
            }
        }

        public async ValueTask<ITopAlbumsResult> GetTopAlbumsAsync(string lastFmUsername, LastFmPeriod period)
        {
            var queryString = new[] {
                "method=user.gettopalbums",
                $"user={lastFmUsername}",
                $"api_key={_options.CurrentValue.LastFmApiKey}",
                $"period={_lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
                "format=json",
                "page=1",
                "limit=10"
            };

            var response = await _httpClient.GetAsync($"https://ws.audioscrobbler.com/2.0/?{string.Join('&', queryString)}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                var albums = jsonDocument.RootElement.GetProperty("topalbums").GetProperty("album");

                return new TopAlbumsResult(albums.EnumerateArray().Select(a =>
                {
                    var images = a.GetProperty("image").EnumerateArray().ToList();

                    return new TopAlbum(
                        Name: a.GetProperty("name").GetString()!,
                        AlbumUrl: new Uri(a.GetProperty("url").GetString()!),
                        AlbumImageUrl: images.Any(i => i.GetProperty("size").GetString() == "large" && !string.IsNullOrEmpty(i.GetProperty("#text").GetString())) ?
                            new Uri(images.First(i => i.GetProperty("size").GetString() == "large").GetProperty("#text").GetString()!) :
                            null,
                        PlayCount: int.Parse(a.GetProperty("playcount").GetString()!),
                        ArtistName: a.GetProperty("artist").GetProperty("name").GetString()!,
                        ArtistUrl: new Uri(a.GetProperty("artist").GetProperty("url").GetString()!)
                    );
                }).ToList());
            }
            else
            {
                try
                {
                    var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    var status = (LastResponseStatus)jsonDocument.RootElement.GetProperty("error").GetUInt16();
                    return new LastFmGenericErrorResult(status.ToString());
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Unhandled error when parsing json in Last.fm error response ({response.StatusCode}):");
                    return new LastFmGenericErrorResult(null);
                }
            }
        }
    }
}
