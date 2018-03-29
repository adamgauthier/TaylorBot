'use strict';

const { GlobalPaths } = require('globalobjects');

const Interval = require(GlobalPaths.Interval);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const YoutubeModule = require(GlobalPaths.YoutubeModule);

const intervalTime = 60000;

class YoutubeInterval extends Interval {
    constructor() {
        super(intervalTime);
    }

    async interval(taylorbot) {
        const youtubeChannels = await taylorbot.database.getYoutubeChannels();
        const it = youtubeChannels.entries();
        this.checkSingleYoutube(taylorbot, it);
    }

    async checkSingleYoutube(taylorbot, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, playlist_id, last_video_id } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const video = await YoutubeModule.getLatestVideo(playlist_id);
            const { videoId } = video.resourceId;

            if (videoId !== last_video_id) {
                Log.info(`New Youtube Video for playlistId '${playlist_id}', ${Format.guildChannel(channel, '#name (#id), #gName (#gId)')}: ${videoId}.`);
                await taylorbot.sendEmbed(channel, YoutubeModule.getRichEmbed(video));
                await taylorbot.database.updateYoutube(playlist_id, guild_id, channel_id, videoId);
            }
        }
        catch (e) {
            Log.error(`Checking Youtube Videos for playlistId '${playlist_id}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleYoutube(taylorbot, iterator);
        }
    }
}

module.exports = YoutubeInterval;