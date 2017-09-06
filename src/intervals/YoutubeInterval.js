'use strict';

const rp = require('request-promise');
const RichEmbed = require('discord.js').RichEmbed;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Config = require(GlobalPaths.Config);
const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const StringUtil = require(GlobalPaths.StringUtil);

const intervalTime = 60000;
const rpOptions = {
    'uri': 'https://www.googleapis.com/youtube/v3/playlistItems',
    'qs': {
        'part': 'snippet',
        'key': Config.googleAPIKey
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

        try {
            const guild = taylorbot.resolveGuild(current.guildId);
            if (!guild) throw new Error(`Guild ID '${current.guildId}' could not be resolved`);

            const channel = guild.channels.get(current.channelId);
            if (!channel) throw new Error(`Channel ID '${current.channelId}' could not be resolved`);

            rpOptions.qs.playlistId = current.playlistId;
            const body = await rp(rpOptions);

            const video = body.items[0].snippet;
            const link = `https://youtu.be/${video.resourceId.videoId}`;
            if (link !== current.lastLink) {
                taylorbot.sendEmbed(channel, YoutubeInterval.getRichEmbed(video));
                database.updateYoutube(link, current.playlistId, current.guildId);                
            }
        }
        catch (e) {
            console.error(`ERR: Checking Youtube Videos for playlistId '${current.playlistId}' for guild ${current.guildId}: ${e}.`);
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