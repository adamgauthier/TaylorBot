'use strict';

const Interval = require('../structures/Interval.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');
const YoutubeModule = require('../modules/google/YoutubeModule.js');

const intervalTime = 60000;

class YoutubeInterval extends Interval {
    constructor() {
        super(intervalTime);
    }

    async interval(client) {
        const youtubeChannels = await client.master.database.youtubeCheckers.getAll();
        const it = youtubeChannels.entries();
        this.checkSingleYoutube(client, it);
    }

    async checkSingleYoutube(client, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, playlist_id, last_video_id } = current;

        try {
            const guild = client.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const video = await YoutubeModule.getLatestVideo(playlist_id);
            const { videoId } = video.resourceId;

            if (videoId !== last_video_id) {
                Log.info(`New Youtube Video for playlistId '${playlist_id}', ${Format.guildChannel(channel)}: ${videoId}.`);
                await client.sendEmbed(channel, YoutubeModule.getEmbed(video));
                await client.master.database.youtubeCheckers.update(playlist_id, guild_id, channel_id, videoId);
            }
        }
        catch (e) {
            Log.error(`Checking Youtube Videos for playlistId '${playlist_id}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleYoutube(client, iterator);
        }
    }
}

module.exports = YoutubeInterval;