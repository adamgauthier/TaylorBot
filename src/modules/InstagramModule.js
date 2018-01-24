'use strict';

const rp = require('request-promise');
const RichEmbed = require('discord.js').RichEmbed;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const StringUtil = require(GlobalPaths.StringUtil);

const instagramBaseURL = 'https://www.instagram.com/';
const rpOptions = {
    'baseUrl': instagramBaseURL,
    'json': true,
    'headers': {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    }
};

class InstagramModule {
    static async getLatestPost(instagramUsername) {
        const options = { ...rpOptions, 'uri': `${instagramUsername}/?__a=1` };
        const body = await rp(options);

        const { user } = body;

        if (user.is_private)
            throw new Error('User is private');

        const { media } = user;

        if (media.nodes.length <= 0)
            throw new Error('Media list was empty');

        const item = media.nodes[0];

        return { item, user };
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
                'icon_url': 'https://www.instagram.com/static/images/ico/apple-touch-icon-76x76-precomposed.png/932e4d9af891.png'
            },
            'color': 0xbc2a8d
        });

        let title = item.caption ? StringUtil.shrinkString(item.caption, 65, ' ...') : '[No Caption]';

        re.setTitle(title);
        re.setThumbnail(item.thumbnail_src);
        return re;
    }
}

module.exports = InstagramModule;