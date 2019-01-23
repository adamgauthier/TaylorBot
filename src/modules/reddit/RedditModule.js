'use strict';

const fetch = require('node-fetch');
const { MessageEmbed } = require('discord.js');

const StringUtil = require('../StringUtil.js');

const REDDIT_ICON_URL = 'https://i.imgur.com/HbUa6WQ.png';
const SPOILER_THUMBNAIL_URL = 'https://i.imgur.com/oCFUKo7.png';
const LINK_THUMBNAIL_URL = 'https://i.imgur.com/Z22fPpC.png';

class RedditModule {
    static async getLatestPost(subreddit) {
        const body = await fetch(`https://www.reddit.com/r/${subreddit}/new/.json`).then(res => res.json());

        const post = body.data.children[0].data;
        return post;
    }

    static getSubredditAbout(subreddit) {
        return fetch(`https://www.reddit.com/r/${subreddit}/about/.json`).then(res => res.json());
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
                'icon_url': REDDIT_ICON_URL
            },
            'color': 0xFF5700
        });


        if (post.is_self) {
            re.setDescription(post.spoiler ? '[Spoiler]' : StringUtil.shrinkString(post.selftext, 400, '(...)'));
        }
        else {
            re.setThumbnail(post.spoiler ?
                SPOILER_THUMBNAIL_URL :
                post.domain === 'i.redd.it' ? post.url :
                    post.thumbnail === 'default' ? LINK_THUMBNAIL_URL : post.thumbnail
            );
            re.setDescription(`🔺 ${StringUtil.plural(post.score, 'point', '`')}, ${StringUtil.plural(post.num_comments, 'comment', '`')} 💬`);
        }

        return re;
    }
}

module.exports = RedditModule;