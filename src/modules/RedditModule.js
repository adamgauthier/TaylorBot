'use strict';

const rp = require('request-promise');
const { RichEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

const StringUtil = require(GlobalPaths.StringUtil);

const redditBaseURL = 'https://www.reddit.com/r/';
const rpOptions = {
    'baseUrl': redditBaseURL,
    'json': true,
    'headers': {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    }
};

class RedditModule {
    static async getLatestPost(subreddit) {
        const options = { ...rpOptions, 'uri': `${subreddit}/new/.json` };
        const body = await rp(options);

        const post = body.data.children[0].data;
        return post;
    }

    static getRichEmbed(post) {
        const re = new RichEmbed({
            'title': StringUtil.shrinkString(post.title, 65, ' ...'),
            'url': `https://redd.it/${post.id}`,
            'timestamp': new Date(post.created_utc * 1000),
            'author': {
                'name': `/r/${post.subreddit}`,
                'url': `${redditBaseURL}${post.subreddit}`
            },
            'footer': {
                'text': `/u/${post.author}`,
                'icon_url': 'http://i.imgur.com/HbUa6WQ.png'
            },
            'color': 0xFF5700
        });


        if (post.is_self) {
            re.setThumbnail('http://i.imgur.com/QEi1hXM.png');
            re.setDescription(StringUtil.shrinkString(post.selftext, 400, '(...)'));
        }
        else {
            re.setThumbnail(post.thumbnail);
            re.setDescription(`🔺 \`${post.score}\` points, \`${post.num_comments}\` comments 💬`);
        }

        return re;
    }
}

module.exports = RedditModule;