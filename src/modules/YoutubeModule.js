'use strict';

const rp = require('request-promise');
const { MessageEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

const { googleAPIKey } = require(GlobalPaths.GoogleConfig);
const StringUtil = require(GlobalPaths.StringUtil);

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

class YoutubeModule {
    static async getLatestVideo(playlistId) {
        const options = { ...rpOptions };
        options.qs.playlistId = playlistId;
        const body = await rp(options);

        const video = body.items[0].snippet;
        return video;
    }

    static getEmbed(video) {
        const re = new MessageEmbed({
            'title': StringUtil.shrinkString(video.title, 65, ' ...'),
            'description': StringUtil.shrinkString(video.description, 200, ' ...'),
            'url': `https://youtu.be/${video.resourceId.videoId}`,
            'timestamp': new Date(video.publishedAt),
            'author': {
                'name': video.channelTitle,
                'url': `https://www.youtube.com/channel/${video.channelId}`
            },
            'footer': {
                'text': 'YouTube',
                'icon_url': 'http://i.imgur.com/ZQUERxd.png'
            },
            'thumbnail': video.thumbnails.medium,
            'color': 0xe52d27
        });

        return re;
    }
}

module.exports = YoutubeModule;