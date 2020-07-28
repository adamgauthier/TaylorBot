using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.Domain
{
    public class LastFmErrorResult : IMostRecentScrobbleResult, ITopArtistsResultResult
    {
        public string Error { get; }

        public LastFmErrorResult(string error)
        {
            Error = error;
        }
    }

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

    public interface ITopArtistsResultResult { }

    public class TopArtistsResult : ITopArtistsResultResult
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

    public interface ILastFmClient
    {
        ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername);
        ValueTask<ITopArtistsResultResult> GetTopArtistsAsync(string lastFmUsername, LastFmPeriod period);
    }
}
