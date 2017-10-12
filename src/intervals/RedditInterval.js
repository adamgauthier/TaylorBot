'use strict';

const rp = require('request-promise');
const RichEmbed = require('discord.js').RichEmbed;

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const StringUtil = require(GlobalPaths.StringUtil);
const Log = require(GlobalPaths.Logger);

const intervalTime = 60000;
const redditBaseURL = 'https://www.reddit.com/r/';
const rpOptions = {
    'baseUrl': redditBaseURL,
    'json': true,
    'headers': {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    }
};

class RedditInterval extends Interval {
    constructor() {
        super(intervalTime, async () => {
            const reddits = await database.getReddits();
            const it = reddits.entries();
            this.checkSingleReddit(it);
        });
    }

    async checkSingleReddit(iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];

        try {
            const guild = taylorbot.resolveGuild(current.guildId);
            if (!guild) throw new Error(`Guild ID '${current.guildId}' could not be resolved`);

            const channel = guild.channels.get(current.channelId);
            if (!channel) throw new Error(`Channel ID '${current.channelId}' could not be resolved`);

            const options = Object.assign({ 'uri': `${current.subreddit}/new/.json` }, rpOptions);
            const body = await rp(options);

            const post = body.data.children[0].data;
            const link = `https://redd.it/${post.id}`;
            if (link !== current.lastLink && post.created_utc > current.lastCreated) {
                taylorbot.sendEmbed(channel, RedditInterval.getRichEmbed(post));
                database.updateReddit(link, post.created_utc, current.subreddit, current.guildId);                
            }
        } 
        catch (e) {
            Log.error(`Checking Reddit Posts for subreddit '${current.subreddit}' for guild ${current.guildId}: ${e}.`);
        }
        finally {
            this.checkSingleReddit(iterator);
        }
    }

    static getRichEmbed(item) {
        const re = new RichEmbed({
            'title': StringUtil.shrinkString(item.title, 65, ' ...'),
            'url': `https://redd.it/${item.id}`,
            'timestamp': new Date(item.created_utc * 1000),
            'author': {
                'name': `/r/${item.subreddit}`,
                'url': `${redditBaseURL}${item.subreddit}`
            },
            'footer': {
                'text': `/u/${item.author}`,
                'icon_url': 'http://i.imgur.com/HbUa6WQ.png'
            },
            'color': 0xFF5700
        });


        if (item.is_self) {
            re.setThumbnail('http://i.imgur.com/QEi1hXM.png');
            re.setDescription(StringUtil.shrinkString(item.selftext, 400, '(...)'));
        }
        else {
            re.setThumbnail(item.thumbnail);
            re.setDescription(`\uD83D\uDD3A \`${item.score}\` points, \`${item.num_comments}\` comments \uD83D\uDCAC`);
        }

        return re;
    }
}

module.exports = new RedditInterval();