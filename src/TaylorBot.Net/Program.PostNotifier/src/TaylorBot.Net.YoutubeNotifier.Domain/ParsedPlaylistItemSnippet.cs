using Google.Apis.YouTube.v3.Data;
using System;

namespace TaylorBot.Net.YoutubeNotifier.Domain
{
    public class ParsedPlaylistItemSnippet
    {
        public PlaylistItemSnippet Snippet { get; }
        public DateTimeOffset? PublishedAt { get; }

        public ParsedPlaylistItemSnippet(PlaylistItemSnippet playlistItemSnippet)
        {
            Snippet = playlistItemSnippet;

            if (DateTimeOffset.TryParse(playlistItemSnippet.PublishedAtRaw, out var parsed))
            {
                PublishedAt = parsed;
            }
            else
            {
                PublishedAt = null;
            }
        }
    }
}
