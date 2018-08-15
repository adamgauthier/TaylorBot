'use strict';

const fetch = require('node-fetch');
const { MessageEmbed } = require('discord.js');

const StringUtil = require('./StringUtil.js');

class RedditModule {
    static async getLatestPost(subreddit) {
        const body = await fetch(`https://www.reddit.com/r/${subreddit}/new/.json`).then(res => res.json());

        const post = body.data.children[0].data;
        return post;
    }

    static getEmbed(post) {
        const re = new MessageEmbed({
            'title': StringUtil.shrinkString(post.title, 65, ' ...'),
            'url': `https://redd.it/${post.id}`,
            'timestamp': new Date(post.created_utc * 1000),
            'author': {
                'name': `r/${post.subreddit}`,
                'url': `https://www.reddit.com/r/${post.subreddit}`
            },
            'footer': {
                'text': `u/${post.author}`,
                'icon_url': 'http://i.imgur.com/HbUa6WQ.png'
            },
            'color': 0xFF5700
        });


        if (post.is_self) {
            re.setDescription(post.spoiler ? '[Spoiler]' : StringUtil.shrinkString(post.selftext, 400, '(...)'));
        }
        else {
            re.setThumbnail(post.spoiler ? 'https://i.imgur.com/oCFUKo7.png' : post.thumbnail);
            re.setDescription(`🔺 ${StringUtil.plural(post.score, 'point', '`')}, ${StringUtil.plural(post.num_comments, 'comment', '`')} 💬`);
        }

        return re;
    }
}

module.exports = RedditModule;