'use strict';

const stripIndents = require('common-tags').stripIndents;
const rp = require('request-promise');
const moment = require('moment');
const RichEmbed = require('discord.js').RichEmbed;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const StringUtil = require(GlobalPaths.StringUtil);
const Log = require(GlobalPaths.Logger);

const intervalTime = 60000;
const instagramBaseURL = 'https://www.instagram.com/';
const rpOptions = {
    'baseUrl': instagramBaseURL,
    'json': true,
    'headers': {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    }
};

class InstagramInterval extends Interval {
    constructor() {
        super(intervalTime, async () => {
            const instagrams = await database.getInstagrams();
            const it = instagrams.entries();
            this.checkSingleInstagram(it);
        });
    }

    async checkSingleInstagram(iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];

        try {
            const guild = taylorbot.resolveGuild(current.serverId);
            if (!guild) throw new Error(`Guild ID '${current.serverId}' could not be resolved`);

            const channel = guild.channels.get(current.channelId);
            if (!channel) throw new Error(`Channel ID '${current.channelId}' could not be resolved`);

            const options = Object.assign({ 'uri': `${current.instagramUsername}/media` }, rpOptions);
            const body = await rp(options);

            if (body.status !== 'ok') 
                throw new Error(`The returned JSON object status property was not ok (${body.status})`);
            if (body.items.length <= 0)
                throw new Error('Items list was empty (user is private or has no posts)');

            const item = body.items[0];
            if (item.link !== current.lastLink) {
                taylorbot.sendEmbed(channel, InstagramInterval.getRichEmbed(item));
                database.updateInstagram(item.link, current.instagramUsername, current.serverId);                
            }
        } 
        catch (e) {
            Log.error(`Checking Instagram Posts for user '${current.instagramUsername}' for guild ${current.serverId}: ${e}.`);
        }
        finally {
            this.checkSingleInstagram(iterator);
        }
    }

    static getRichEmbed(item) {
        const re = new RichEmbed({
            'description': `\`${item.likes.count}\` likes \u2764, \`${item.comments.count}\` comments \uD83D\uDCAC`,
            'thumbnail': item.images.thumbnail,
            'url': item.link,
            'timestamp': new Date(item.created_time * 1000),
            'author': {
                'name': (item.user.full_name ? item.user.full_name : item.user.username),
                'url': instagramBaseURL + item.user.username,
                'icon_url': item.user.profile_picture
            },
            'footer': {
                'text': 'Instagram',
                'icon_url': 'https://instagramstatic-a.akamaihd.net/h1/images/ico/apple-touch-icon-76x76-precomposed.png/932e4d9af891.png'
            },
            'color': 0xbc2a8d
        });

        let title = item.caption ? StringUtil.shrinkString(item.caption.text, 65, ' ...') : '[No Caption]';

        re.setTitle(title);
        return re;
    }

    static getText(item) {
        return stripIndents`
        **\uD83D\uDCF7 Instagram \uD83D\uDCF7
        New post by ${item.user.full_name}**:
        ${item.caption ? item.caption.text : '[No Caption]'}
        __Posted on:__ ${moment(item.created_time, 'X').format('MMMM Do, YYYY \\at H:mm:ss')} UTC
        \`${item.likes.count}\` likes \u2665, \`${item.comments.count}\` comments \uD83D\uDCAC
        __Link to post:__ <${item.link}>
        __Link to media:__ ${item.images.standard_resolution.url}`;
    }
}

module.exports = new InstagramInterval();