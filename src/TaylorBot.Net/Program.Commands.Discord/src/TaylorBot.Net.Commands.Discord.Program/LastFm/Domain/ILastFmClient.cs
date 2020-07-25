using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.LastFm.Domain
{
    public interface IMostRecentScrobbleResult { }

    public class LastFmErrorResult : IMostRecentScrobbleResult
    {
        public string Error { get; }

        public LastFmErrorResult(string error)
        {
            Error = error;
        }
    }

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

    public interface ILastFmClient
    {
        ValueTask<IMostRecentScrobbleResult> GetMostRecentScrobbleAsync(string lastFmUsername);
    }
}
