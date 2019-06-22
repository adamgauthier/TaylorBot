using Dapper;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Options;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.YoutubeNotifier.Domain;
using TaylorBot.Net.YoutubeNotifier.Infrastructure.Models;

namespace TaylorBot.Net.YoutubeNotifier.Infrastructure
{
    public class YoutubeCheckerRepository : PostgresRepository, IYoutubeCheckerRepository
    {
        public YoutubeCheckerRepository(IOptionsMonitor<DatabaseConnectionOptions> optionsMonitor) : base(optionsMonitor)
        {
        }

        public async Task<IEnumerable<YoutubeChecker>> GetYoutubeCheckersAsync()
        {
            using (var connection = Connection)
            {
                connection.Open();
                var checkers = await connection.QueryAsync<YoutubeCheckerDto>("SELECT * FROM checkers.youtube_checker;");
                return checkers.Select(checker => new YoutubeChecker(
                    guildId: new SnowflakeId(checker.guild_id),
                    channelId: new SnowflakeId(checker.channel_id),
                    playlistId: checker.playlist_id,
                    lastVideoId: checker.last_video_id
                ));
            }
        }

        public async Task UpdateLastPostAsync(YoutubeChecker youtubeChecker, PlaylistItemSnippet youtubePost)
        {
            using (var connection = Connection)
            {
                connection.Open();
                await connection.ExecuteAsync(
                    @"UPDATE checkers.youtube_checker SET last_video_id = @LastVideoId
                      WHERE playlist_id = @PlaylistId AND guild_id = @GuildId AND channel_id = @ChannelId;",
                    new
                    {
                        PlaylistId = youtubeChecker.PlaylistId,
                        GuildId = youtubeChecker.GuildId.ToString(),
                        ChannelId = youtubeChecker.ChannelId.ToString(),
                        LastVideoId = youtubePost.ResourceId.VideoId
                    }
                );
            }
        }
    }
}
