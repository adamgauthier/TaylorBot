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
        const { guild_id, channel_id, instagram_username, last_post_code } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const options = Object.assign({ 'uri': `${instagram_username}/?__a=1` }, rpOptions);
            const body = await rp(options);

            const { user } = body;

            if (user.is_private)
                throw new Error('User is private');

            const { media } = user;

            if (media.nodes.length <= 0)
                throw new Error('Media list was empty');

            const item = media.nodes[0];            
            if (item.code !== last_post_code) {
                await taylorbot.sendEmbed(channel, InstagramInterval.getRichEmbed(item, user));
                await database.updateInstagram(instagram_username, guild_id, channel_id, item.code);
            }
        }
        catch (e) {
            Log.error(`Checking Instagram Posts for user '${instagram_username}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleInstagram(iterator);
        }
    }

    static getRichEmbed(item, user) {
        const re = new RichEmbed({
            'description': `\`${item.likes.count}\` likes \u2764, \`${item.comments.count}\` comments \uD83D\uDCAC`,
            'url': `https://www.instagram.com/p/${item.code}/`,
            'timestamp': new Date(item.date * 1000),
            'author': {
                'name': (user.full_name ? user.full_name : user.username),
                'url': instagramBaseURL + user.username,
                'icon_url': user.profile_pic_url
            },
            'footer': {
                'text': 'Instagram',
                'icon_url': 'https://instagramstatic-a.akamaihd.net/h1/images/ico/apple-touch-icon-76x76-precomposed.png/932e4d9af891.png'
            },
            'color': 0xbc2a8d
        });

        let title = item.caption ? StringUtil.shrinkString(item.caption, 65, ' ...') : '[No Caption]';

        re.setTitle(title);
        re.setThumbnail(item.thumbnail_src);
        return re;
    }
}

module.exports = new InstagramInterval();