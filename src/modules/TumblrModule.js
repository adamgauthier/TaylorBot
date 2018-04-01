'use strict';

const { MessageEmbed } = require('discord.js');
const tumblr = require('tumblr.js');
const { GlobalPaths } = require('globalobjects');

const TumblrConfig = require(GlobalPaths.TumblrConfig);
const StringUtil = require(GlobalPaths.StringUtil);

const client = tumblr.createClient({
    credentials: TumblrConfig.tumblrCredentials,
    returnPromises: true
});

class TumblrModule {
    static async getLatestPost(tumblrUser) {
        const data = await TumblrModule.getDataFromUser(tumblrUser);

        return { 'post': data.posts[0], 'blog': data.blog };
    }

    static getDataFromUser(tumblrUser) {
        return client.blogPosts(tumblrUser, { 'filter': 'text' });
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
                'icon_url': 'http://i.imgur.com/vKz0iHQ.png'
            },
            'color': 0x36465D
        });

        let description = '';
        let thumbnail = '';

        switch (post.type) {
            case 'link':
                if (post.description) description = post.description;
                else if (post.excerpt) description = post.excerpt;
                thumbnail = 'http://i.imgur.com/1zeAyat.png';
                break;

            case 'video':
                description = post.permalink_url;
                thumbnail = post.thumbnail_url ? post.thumbnail_url : 'http://i.imgur.com/R8x5Qp6.png';
                break;

            case 'photo':
                if (post.tags && post.tags.length > 0) description = post.tags.map(e => `#${e}`).join(' ');
                else if (post.image_permalink) description = post.image_permalink;
                const altSizes = post.photos[0].alt_sizes;
                thumbnail = altSizes[altSizes.length - 2].url;
                break;

            case 'text':
                thumbnail = 'http://i.imgur.com/QEi1hXM.png';
            case 'chat':
                description = post.body;
                break;

            case 'quote':
                if (post.tags && post.tags.length > 0) description = post.tags.map(e => `#${e}`).join(' ');
                thumbnail = 'http://i.imgur.com/Xz5GKKh.png';
                break;

            case 'audio':
                thumbnail = 'http://i.imgur.com/NM5J5SD.png';
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