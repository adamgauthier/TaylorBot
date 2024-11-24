using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Http;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Infrastructure;

public sealed class InflatableLastFmClient(
    ILogger<InflatableLastFmClient> logger,
    IOptionsMonitor<LastFmOptions> options,
    HttpClient httpClient,
    LastfmClient client,
    LastFmPeriodStringMapper lastFmPeriodStringMapper) : ILastFmClient
{
    public async ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername)
    {
        var response = await client.User.GetRecentScrobbles(
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
        else if (response.Status == LastResponseStatus.MissingParameters)
        {
            return new LastFmUserNotFound($"{response.Status}", lastFmUsername);
        }
        else
        {
            return new LastFmGenericErrorResult($"{response.Status}");
        }
    }

    private static LastStatsTimeSpan MapPeriodToTimeSpan(LastFmPeriod period)
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
        var response = await client.User.GetTopArtists(
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
        else if (response.Status == LastResponseStatus.MissingParameters)
        {
            return new LastFmUserNotFound($"{response.Status}", lastFmUsername);
        }
        else
        {
            return new LastFmGenericErrorResult($"{response.Status}");
        }
    }

    public async ValueTask<ITopTracksResult> GetTopTracksAsync(string lastFmUsername, LastFmPeriod period)
    {
        var queryString = new[] {
            "method=user.gettoptracks",
            $"user={lastFmUsername}",
            $"api_key={options.CurrentValue.LastFmApiKey}",
            $"period={lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
            "format=json",
            "page=1",
            "limit=10"
        };

        return await httpClient.ReadJsonWithErrorLogging<TopTracksResponse, ITopTracksResult>(
            c => c.GetAsync($"https://ws.audioscrobbler.com/2.0/?{string.Join('&', queryString)}"),
            handleSuccessAsync: success => Task.FromResult(HandleTopTracksSuccess(success)),
            handleErrorAsync: error => Task.FromResult<ITopTracksResult>(HandleLastFmHttpError(error, lastFmUsername)),
            logger);
    }

    private ITopTracksResult HandleTopTracksSuccess(HttpSuccess<TopTracksResponse> result)
    {
        var tracks = result.Parsed.toptracks.track;

        return new TopTracksResult(tracks.Select(t => new TopTrack(
            Name: t.name,
            TrackUrl: new(t.url),
            PlayCount: int.Parse(t.playcount),
            ArtistName: t.artist.name,
            ArtistUrl: new(t.artist.url)
        )).ToList());
    }

    public record TopTracksResponse(TopTracksResponse.TopTracks toptracks)
    {
        public record TopTracks(IReadOnlyList<Track> track);

        public record Track(
            string name,
            string url,
            string playcount,
            Artist artist
        );

        public record Artist(string name, string url);
    }

    public async ValueTask<ITopAlbumsResult> GetTopAlbumsAsync(string lastFmUsername, LastFmPeriod period)
    {
        var queryString = new[] {
            "method=user.gettopalbums",
            $"user={lastFmUsername}",
            $"api_key={options.CurrentValue.LastFmApiKey}",
            $"period={lastFmPeriodStringMapper.MapLastFmPeriodToUrlString(period)}",
            "format=json",
            "page=1",
            "limit=10"
        };

        return await httpClient.ReadJsonWithErrorLogging<TopAlbumsResponse, ITopAlbumsResult>(
            c => c.GetAsync($"https://ws.audioscrobbler.com/2.0/?{string.Join('&', queryString)}"),
            handleSuccessAsync: success => Task.FromResult(HandleTopAlbumsSuccess(success)),
            handleErrorAsync: error => Task.FromResult<ITopAlbumsResult>(HandleLastFmHttpError(error, lastFmUsername)),
            logger);
    }

    private ITopAlbumsResult HandleTopAlbumsSuccess(HttpSuccess<TopAlbumsResponse> result)
    {
        var albums = result.Parsed.topalbums.album;

        return new TopAlbumsResult(albums.Select(a =>
        {
            var images = a.image;

            return new TopAlbum(
                Name: a.name,
                AlbumUrl: new(a.url),
                AlbumImageUrl: images.Any(i => i.size == "large" && !string.IsNullOrEmpty(i.text))
                    ? new Uri(images.First(i => i.size == "large").text)
                    : null,
                PlayCount: int.Parse(a.playcount),
                ArtistName: a.artist.name,
                ArtistUrl: new(a.artist.url)
            );
        }).ToList());
    }

    public record TopAlbumsResponse(TopAlbumsResponse.TopAlbums topalbums)
    {
        public record TopAlbums(IReadOnlyList<Album> album);

        public record Album(
            string name,
            string url,
            string playcount,
            Artist artist,
            IReadOnlyList<AlbumImage> image
        );

        public record AlbumImage(string size, [property: JsonPropertyName("#text")] string text);

        public record Artist(string name, string url);
    }

    private LastFmGenericErrorResult HandleLastFmHttpError(HttpError error, string lastFmUsername)
    {
        if (error.Content != null)
        {
            var jsonDocument = JsonDocument.Parse(error.Content);

            var status = jsonDocument.RootElement.TryGetProperty("error", out var errorCode)
                ? (LastResponseStatus?)errorCode.GetUInt16()
                : null;

            if (jsonDocument.RootElement.TryGetProperty("message", out var message) &&
                message.GetString()?.Contains("user not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new LastFmUserNotFound(status?.ToString(), lastFmUsername);
            }

            return new LastFmGenericErrorResult(status?.ToString());
        }
        else
        {
            return new LastFmGenericErrorResult(null);
        }
    }
}
