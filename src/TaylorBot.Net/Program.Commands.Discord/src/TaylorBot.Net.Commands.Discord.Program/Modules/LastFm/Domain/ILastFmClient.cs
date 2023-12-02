namespace TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;

public record LastFmGenericErrorResult(string? Error) : IMostRecentScrobbleResult, ITopArtistsResult, ITopTracksResult, ITopAlbumsResult;

public record LastFmLogInRequiredErrorResult : IMostRecentScrobbleResult;

public interface IMostRecentScrobbleResult { }

public record MostRecentScrobbleResult(int TotalScrobbles, MostRecentScrobble? MostRecentTrack) : IMostRecentScrobbleResult;

public record ScrobbleArtist(string Name, string Url);

public record MostRecentScrobble(string TrackName, string TrackUrl, string? TrackImageUrl, ScrobbleArtist Artist, bool IsNowPlaying);

public interface ITopArtistsResult { }

public record TopArtistsResult(IReadOnlyList<TopArtist> TopArtists) : ITopArtistsResult;

public record TopArtist(string Name, Uri ArtistUrl, int PlayCount);

public enum LastFmPeriod
{
    SevenDay,
    OneMonth,
    ThreeMonth,
    SixMonth,
    TwelveMonth,
    Overall
}

public interface ITopTracksResult { }

public record TopTracksResult(IReadOnlyList<TopTrack> TopTracks) : ITopTracksResult;

public record TopTrack(string Name, Uri TrackUrl, int PlayCount, string ArtistName, Uri ArtistUrl);

public interface ITopAlbumsResult { }

public record TopAlbumsResult(IReadOnlyList<TopAlbum> TopAlbums) : ITopAlbumsResult;

public record TopAlbum(string Name, Uri AlbumUrl, Uri? AlbumImageUrl, int PlayCount, string ArtistName, Uri ArtistUrl);

public interface ILastFmClient
{
    ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername);
    ValueTask<ITopArtistsResult> GetTopArtistsAsync(string lastFmUsername, LastFmPeriod period);
    ValueTask<ITopTracksResult> GetTopTracksAsync(string lastFmUsername, LastFmPeriod period);
    ValueTask<ITopAlbumsResult> GetTopAlbumsAsync(string lastFmUsername, LastFmPeriod period);
}
