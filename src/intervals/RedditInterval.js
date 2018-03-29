'use strict';

const { GlobalPaths } = require('globalobjects');

const Interval = require(GlobalPaths.Interval);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const RedditModule = require(GlobalPaths.RedditModule);

const intervalTime = 60000;

class RedditInterval extends Interval {
    constructor() {
        super(intervalTime);
    }

    async interval(taylorbot) {
        const reddits = await taylorbot.database.getReddits();
        const it = reddits.entries();
        this.checkSingleReddit(taylorbot, it);
    }

    async checkSingleReddit(taylorbot, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, subreddit, last_post_id, last_created } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const post = await RedditModule.getLatestPost(subreddit);

            if (post.id !== last_post_id && post.created_utc > last_created) {
                Log.info(`New Reddit Post for subreddit '${subreddit}', ${Format.guildChannel(channel, '#name (#id), #gName (#gId)')}: ${post.id}.`);
                await taylorbot.sendEmbed(channel, RedditModule.getRichEmbed(post));
                await taylorbot.database.updateReddit(subreddit, guild_id, channel_id, post.id, post.created_utc);                
            }
        } 
        catch (e) {
            Log.error(`Checking Reddit Posts for subreddit '${subreddit}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleReddit(taylorbot, iterator);
        }
    }
}

module.exports = RedditInterval;