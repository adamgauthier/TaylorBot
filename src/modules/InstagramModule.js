'use strict';

const fetch = require('node-fetch');
const { MessageEmbed } = require('discord.js');
const { Paths } = require('globalobjects');

const StringUtil = require(Paths.StringUtil);

class InstagramModule {
    static async getLatestPost(instagramUsername) {
        const body = await fetch(`https://www.instagram.com/${instagramUsername}/?__a=1`).then(res => res.json());

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

    static getEmbed(item, user) {
        const re = new MessageEmbed({
            'description': `\`${item.edge_liked_by.count}\` likes ❤, \`${item.edge_media_to_comment.count}\` comments 💬`,
            'url': `https://www.instagram.com/p/${item.shortcode}/`,
            'timestamp': new Date(item.taken_at_timestamp * 1000),
            'author': {
                'name': (user.full_name ? user.full_name : user.username),
                'url': `https://www.instagram.com/${user.username}`,
                'icon_url': user.profile_pic_url
            },
            'footer': {
                'text': 'Instagram',
                'icon_url': 'https://www.instagram.com/static/images/ico/apple-touch-icon-76x76-precomposed.png/932e4d9af891.png'
            },
            'color': 0xbc2a8d
        });

        const { edges } = item.edge_media_to_caption;

        const title = edges.length > 0 ? StringUtil.shrinkString(edges[0].node.text, 65, ' ...') : '[No Caption]';

        re.setTitle(title);
        re.setThumbnail(item.thumbnail_src);
        return re;
    }
}

module.exports = InstagramModule;