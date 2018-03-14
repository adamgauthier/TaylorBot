'use strict';

const rp = require('request-promise');
const { RichEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

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

        const { user } = body.graphql;

        if (user.is_private)
            throw new Error('User is private');

        const { edge_owner_to_timeline_media } = user;

        if (edge_owner_to_timeline_media.count <= 0)
            throw new Error('Media list was empty');

        const { edges } = edge_owner_to_timeline_media;

        const item = edges[0].node;

        return { item, user };
    }

    static getRichEmbed(item, user) {
        const re = new RichEmbed({
            'description': `\`${item.edge_liked_by.count}\` likes ❤, \`${item.edge_media_to_comment.count}\` comments 💬`,
            'url': `https://www.instagram.com/p/${item.shortcode}/`,
            'timestamp': new Date(item.taken_at_timestamp * 1000),
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

        const { edges } = item.edge_media_to_caption;

        let title = edges.length > 0 ? StringUtil.shrinkString(edges[0].node.text, 65, ' ...') : '[No Caption]';

        re.setTitle(title);
        re.setThumbnail(item.thumbnail_src);
        return re;
    }
}

module.exports = InstagramModule;