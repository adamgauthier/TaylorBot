'use strict';

const rp = require('request-promise');
const RichEmbed = require('discord.js').RichEmbed;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const { googleAPIKey } = require(GlobalPaths.GoogleConfig);
const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const StringUtil = require(GlobalPaths.StringUtil);
const Log = require(GlobalPaths.Logger);

const intervalTime = 60000;
const rpOptions = {
    'uri': 'https://www.googleapis.com/youtube/v3/playlistItems',
    'qs': {
        'part': 'snippet',
        'key': googleAPIKey
    },
    'json': true,
    'headers': {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    }
};

class YoutubeInterval extends Interval {
    constructor() {
        super(intervalTime, async () => {
            const youtubeChannels = await database.getYoutubeChannels();
            const it = youtubeChannels.entries();
            this.checkSingleYoutube(it);
        });
    }

    async checkSingleYoutube(iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, playlist_id, last_link } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            rpOptions.qs.playlistId = playlist_id;
            const body = await rp(rpOptions);

            const video = body.items[0].snippet;
            const link = `https://youtu.be/${video.resourceId.videoId}`;
            if (link !== last_link) {
                Log.info(`Detected new Youtube Video for playlistId '${playlist_id}', guild ${guild_id}, channel ${channel_id}: ${link}.`);
                await taylorbot.sendEmbed(channel, YoutubeInterval.getRichEmbed(video));
                await database.updateYoutube(playlist_id, guild_id, channel_id, link);
            }
        }
        catch (e) {
            Log.error(`Checking Youtube Videos for playlistId '${playlist_id}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleYoutube(iterator);
        }
    }

    static getRichEmbed(video) {
        const re = new RichEmbed({
            'title': StringUtil.shrinkString(video.title, 65, ' ...'),
            'description': StringUtil.shrinkString(video.description, 200, ' ...'),
            'url': `https://youtu.be/${video.resourceId.videoId}`,
            'timestamp': new Date(video.publishedAt),
            'author': {
                'name': video.channelTitle,
                'url': `https://www.youtube.com/channel/${video.channelId}`
            },
            'footer': {
                'text': `YouTube`,
                'icon_url': 'http://i.imgur.com/ZQUERxd.png'
            },
            'thumbnail': video.thumbnails.medium,
            'color': 0xe52d27
        });

        return re;
    }
}

module.exports = new YoutubeInterval();