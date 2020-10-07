using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.Domain
{
    public class LastFmGenericErrorResult : IMostRecentScrobbleResult, ITopArtistsResult, ITopTracksResult, ITopAlbumsResult
    {
        public string? Error { get; }

        public LastFmGenericErrorResult(string? error)
        {
            Error = error;
        }
    }

    public class LastFmLogInRequiredErrorResult : IMostRecentScrobbleResult { }

    public interface IMostRecentScrobbleResult { }

    public class MostRecentScrobbleResult : IMostRecentScrobbleResult
    {
        public int TotalScrobbles { get; }
        public MostRecentScrobble? MostRecentTrack { get; }

        public MostRecentScrobbleResult(int totalScrobbles, MostRecentScrobble? mostRecentTrack)
        {
            TotalScrobbles = totalScrobbles;
            MostRecentTrack = mostRecentTrack;
        }
    }
    public class ScrobbleArtist
    {
        public string Name { get; }
        public string Url { get; }

        public ScrobbleArtist(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }

    public class MostRecentScrobble
    {
        public string TrackName { get; }
        public string TrackUrl { get; }
        public string? TrackImageUrl { get; }
        public ScrobbleArtist Artist { get; }
        public bool IsNowPlaying { get; }

        public MostRecentScrobble(string trackName, string trackUrl, string? trackImageUrl, ScrobbleArtist artist, bool isNowPlaying)
        {
            TrackName = trackName;
            TrackUrl = trackUrl;
            TrackImageUrl = trackImageUrl;
            Artist = artist;
            IsNowPlaying = isNowPlaying;
        }
    }

    public interface ITopArtistsResult { }

    public class TopArtistsResult : ITopArtistsResult
    {
        public IReadOnlyList<TopArtist> TopArtists { get; }

        public TopArtistsResult(IReadOnlyList<TopArtist> topArtists)
        {
            TopArtists = topArtists;
        }
    }

    public class TopArtist
    {
        public string Name { get; }
        public Uri ArtistUrl { get; }
        public int PlayCount { get; }

        public TopArtist(string name, Uri artistUrl, int playCount)
        {
            Name = name;
            ArtistUrl = artistUrl;
            PlayCount = playCount;
        }
    }

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

    public class TopTracksResult : ITopTracksResult
    {
        public IReadOnlyList<TopTrack> TopTracks { get; }

        public TopTracksResult(IReadOnlyList<TopTrack> topTracks)
        {
            TopTracks = topTracks;
        }
    }

    public class TopTrack
    {
        public string Name { get; }
        public Uri TrackUrl { get; }
        public int PlayCount { get; }
        public string ArtistName { get; }
        public Uri ArtistUrl { get; }

        public TopTrack(string name, Uri trackUrl, int playCount, string artistName, Uri artistUrl)
        {
            Name = name;
            TrackUrl = trackUrl;
            PlayCount = playCount;
            ArtistName = artistName;
            ArtistUrl = artistUrl;
        }
    }

    public interface ITopAlbumsResult { }

    public class TopAlbumsResult : ITopAlbumsResult
    {
        public IReadOnlyList<TopAlbum> TopAlbums { get; }

        public TopAlbumsResult(IReadOnlyList<TopAlbum> topAlbums)
        {
            TopAlbums = topAlbums;
        }
    }

    public class TopAlbum
    {
        public string Name { get; }
        public Uri AlbumUrl { get; }
        public Uri? AlbumImageUrl { get; }
        public int PlayCount { get; }
        public string ArtistName { get; }
        public Uri ArtistUrl { get; }

        public TopAlbum(string name, Uri albumUrl, Uri? albumImageUrl, int playCount, string artistName, Uri artistUrl)
        {
            Name = name;
            AlbumUrl = albumUrl;
            AlbumImageUrl = albumImageUrl;
            PlayCount = playCount;
            ArtistName = artistName;
            ArtistUrl = artistUrl;
        }
    }

    public interface ILastFmClient
    {
        ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername);
        ValueTask<ITopArtistsResult> GetTopArtistsAsync(string lastFmUsername, LastFmPeriod period);
        ValueTask<ITopTracksResult> GetTopTracksAsync(string lastFmUsername, LastFmPeriod period);
        ValueTask<ITopAlbumsResult> GetTopAlbumsAsync(string lastFmUsername, LastFmPeriod period);
    }
}
