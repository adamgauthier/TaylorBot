using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;

public static class LastFmResponses
{
    private const string RecentUrl = "http://ws.audioscrobbler.com/2.0/?method=user.getRecentTracks&user=taylorswift&api_key=synthetic&format=json&page=1&limit=1&extended=False&disablecachetoken=0";
    public const string ImageUrl = "https://images.example.invalid/album.png";
    public const string ArtistName = "Taylor Swift";
    public const string ArtistUrl = "https://www.last.fm/music/Taylor+Swift";
    public const string TrackName = "All Too Well";
    public const string TrackUrl = "https://www.last.fm/music/Taylor+Swift/_/All+Too+Well";

    public static void RecentError(this ExternalApi api, int error) =>
        api.Json("GET", RecentUrl, new { error, message = "Synthetic Last.fm failure" }, numericQueryParameter: "disablecachetoken");

    public static void NoRecentScrobbles(this ExternalApi api) => Recent(api, []);

    public static void RecentScrobble(this ExternalApi api, bool nowPlaying = true, bool expandedArtist = false)
    {
        JsonObject track = new()
        {
            ["name"] = TrackName,
            ["url"] = TrackUrl,
            ["artist"] = expandedArtist
                ? new JsonObject { ["name"] = ArtistName, ["url"] = ArtistUrl, ["mbid"] = "", ["image"] = Images() }
                : new JsonObject { ["#text"] = ArtistName, ["mbid"] = "" },
            ["album"] = new JsonObject { ["#text"] = "Red", ["mbid"] = "" },
            ["image"] = Images(),
        };
        if (nowPlaying)
            track["@attr"] = new JsonObject { ["nowplaying"] = "true" };
        else
            track["date"] = new JsonObject { ["uts"] = "1767225600", ["#text"] = "01 Jan 2026, 00:00" };
        Recent(api, [track]);
        api.Json("GET", $"https://ws.audioscrobbler.com/2.0/?method=track.getInfo&user=taylorswift&api_key=synthetic&format=json&artist={Uri.EscapeDataString(ArtistName)}&track={Uri.EscapeDataString(TrackName)}",
            new { track = new { userplaycount = "13" } });
    }

    private static void Recent(ExternalApi api, JsonArray tracks) =>
        api.Json("GET", RecentUrl, new JsonObject
        {
            ["recenttracks"] = new JsonObject { ["track"] = tracks, ["@attr"] = Attributes(tracks.Count) },
        }, numericQueryParameter: "disablecachetoken");

    public static void TopAlbums(this ExternalApi api, string album, string artist, int count)
    {
        var entry = JsonSerializer.SerializeToNode(new
        {
            name = album,
            url = AlbumUrl(album, artist),
            playcount = "13",
            artist = new { name = artist, url = MusicUrl(artist) },
        })!.AsObject();
        entry["image"] = Images();
        Top(api, "albums", "album", "1month", entry, count);
    }

    public static void NoTopAlbums(this ExternalApi api) =>
        Top(api, "albums", "album", "7day", new JsonObject(), count: 0);

    public static void TopTracks(this ExternalApi api, string track, string artist, int count) =>
        Top(api, "tracks", "track", "1month", JsonSerializer.SerializeToNode(new
        {
            name = track,
            url = SongUrl(track, artist),
            playcount = "22",
            artist = new { name = artist, url = MusicUrl(artist) },
        })!, count);

    public static void TopArtists(this ExternalApi api, string artist, int count) =>
        Top(api, "artists", "artist", "6month", JsonSerializer.SerializeToNode(new
        {
            name = artist,
            url = MusicUrl(artist),
            playcount = "15",
            mbid = "",
            streamable = "0",
        })!, count);

    private static void Top(ExternalApi api, string plural, string singular, string period, JsonNode entry, int count)
    {
        JsonArray entries = new([.. Enumerable.Range(0, count).Select(_ => entry.DeepClone())]);
        var url = plural == "artists"
            ? $"http://ws.audioscrobbler.com/2.0/?method=user.getTopArtists&user=taylorswift&api_key=synthetic&period={period}&format=json&page=1&limit=10&disablecachetoken=0"
            : $"https://ws.audioscrobbler.com/2.0/?method=user.gettop{plural}&user=taylorswift&api_key=synthetic&period={period}&format=json&page=1&limit=10";
        api.Json("GET", url,
            new JsonObject { [$"top{plural}"] = new JsonObject { [singular] = entries, ["@attr"] = Attributes(count) } },
            numericQueryParameter: plural == "artists" ? "disablecachetoken" : null);
    }

    private static JsonObject Attributes(int count) => new()
    {
        ["user"] = "taylorswift",
        ["page"] = "1",
        ["perPage"] = "10",
        ["totalPages"] = "1",
        ["total"] = $"{count}",
    };

    private static JsonArray Images() =>
    [
        new JsonObject { ["size"] = "small", ["#text"] = "https://images.example.invalid/small.png" },
        new JsonObject { ["size"] = "medium", ["#text"] = "https://images.example.invalid/medium.png" },
        new JsonObject { ["size"] = "large", ["#text"] = ImageUrl },
        new JsonObject { ["size"] = "extralarge", ["#text"] = "https://images.example.invalid/extralarge.png" },
    ];
    public static string MusicUrl(string artist) => $"https://www.last.fm/music/{Uri.EscapeDataString(artist).Replace("%20", "+", StringComparison.Ordinal)}";
    public static string AlbumUrl(string album, string artist) => $"{MusicUrl(artist)}/{Uri.EscapeDataString(album).Replace("%20", "+", StringComparison.Ordinal)}";
    public static string SongUrl(string track, string artist) => $"{MusicUrl(artist)}/_/{Uri.EscapeDataString(track).Replace("%20", "+", StringComparison.Ordinal)}";

    public static IReadOnlyList<byte> Collage(this ExternalApi api)
    {
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Wl6rWQAAAAASUVORK5CYII=");
        api.Json("POST", "https://lastcollage.io/api/collage", new { downloadPath = "api/collage/synthetic.png" });
        api.Bytes("https://lastcollage.io/api/collage/synthetic.png", bytes);
        return bytes;
    }

    public static void CollageWithoutScrobbles(this ExternalApi api) =>
        api.Json("POST", "https://lastcollage.io/api/collage",
            new { message = "The account does not have any scrobbles for the time period specified." }, HttpStatusCode.NotFound);

    public static void CollageError(this ExternalApi api, HttpStatusCode status, string body) =>
        api.Raw("POST", "https://lastcollage.io/api/collage", body, "application/json", status);
}
