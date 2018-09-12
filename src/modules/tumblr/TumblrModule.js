'use strict';

const fetch = require('node-fetch');
const querystring = require('querystring');
const { MessageEmbed } = require('discord.js');

const { consumer_key } = require('../../config/tumblr.json');
const StringUtil = require('../StringUtil');

class TumblrModule {
    static async getLatestPost(tumblrUser, limit) {
        const res = await fetch(`https://api.tumblr.com/v2/blog/${tumblrUser}.tumblr.com/posts?${querystring.stringify({
            api_key: consumer_key,
            filter: 'text',
            limit
        })}`).then(res => res.json());

        const { response } = res;

        return { post: response.posts[0], blog: response.blog };
    }

    static getEmbed(post, blog) {
        const re = new MessageEmbed({
            'url': post.short_url,
            'title': post.summary ? StringUtil.shrinkString(post.summary.replace(/\r?\n/g, ' '), 65, ' ...') : '(no title)',
            'timestamp': new Date(post.timestamp * 1000),
            'author': {
                'name': blog.title,
                'url': blog.url
            },
            'footer': {
                'text': 'Tumblr',
                'icon_url': 'https://i.imgur.com/vKz0iHQ.png'
            },
            'color': 0x36465D
        });

        let description = '';
        let thumbnail = '';

        switch (post.type) {
            case 'link':
                if (post.description) description = post.description;
                else if (post.excerpt) description = post.excerpt;
                thumbnail = 'https://i.imgur.com/1zeAyat.png';
                break;

            case 'video':
                description = post.permalink_url;
                thumbnail = post.thumbnail_url ? post.thumbnail_url : 'https://i.imgur.com/R8x5Qp6.png';
                break;

            case 'photo': {
                if (post.tags && post.tags.length > 0) description = post.tags.map(e => `#${e}`).join(' ');
                else if (post.image_permalink) description = post.image_permalink;
                const altSizes = post.photos[0].alt_sizes;
                thumbnail = altSizes[altSizes.length - 2].url;
                break;
            }

            case 'text':
                thumbnail = 'https://i.imgur.com/QEi1hXM.png';
                description = post.body;
                break;

            case 'chat':
                description = post.body;
                break;

            case 'quote':
                if (post.tags && post.tags.length > 0) description = post.tags.map(e => `#${e}`).join(' ');
                thumbnail = 'https://i.imgur.com/Xz5GKKh.png';
                break;

            case 'audio':
                thumbnail = 'https://i.imgur.com/NM5J5SD.png';
                break;

            case 'answer':
                description = post.answer;
                break;
        }

        if (description)
            re.setDescription(StringUtil.shrinkString(description, 400, ' ...'));

        re.setThumbnail(thumbnail);
        return re;
    }
}

module.exports = TumblrModule;