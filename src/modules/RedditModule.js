'use strict';

const rp = require('request-promise');
const { MessageEmbed } = require('discord.js');
const { Paths } = require('globalobjects');

const StringUtil = require(Paths.StringUtil);

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

    static getEmbed(post) {
        const re = new MessageEmbed({
            'title': StringUtil.shrinkString(post.title, 65, ' ...'),
            'url': `https://redd.it/${post.id}`,
            'timestamp': new Date(post.created_utc * 1000),
            'author': {
                'name': `r/${post.subreddit}`,
                'url': `${redditBaseURL}${post.subreddit}`
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