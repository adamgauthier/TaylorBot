'use strict';

const { Paths } = require('globalobjects');

const Interval = require(Paths.Interval);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const RedditModule = require(Paths.RedditModule);

const intervalTime = 60000;

class RedditInterval extends Interval {
    constructor() {
        super(intervalTime);
    }

    async interval(client) {
        const reddits = await client.master.database.redditCheckers.getAll();
        const it = reddits.entries();
        this.checkSingleReddit(client, it);
    }

    async checkSingleReddit(client, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, subreddit, last_post_id, last_created } = current;

        try {
            const guild = client.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const post = await RedditModule.getLatestPost(subreddit);

            if (post.id !== last_post_id && post.created_utc > last_created) {
                Log.info(`New Reddit Post for subreddit '${subreddit}', ${Format.guildChannel(channel, '#name (#id), #gName (#gId)')}: ${post.id}.`);
                await client.sendEmbed(channel, RedditModule.getEmbed(post));
                await client.master.database.redditCheckers.update(subreddit, guild_id, channel_id, post.id, post.created_utc);
            }
        }
        catch (e) {
            Log.error(`Checking Reddit Posts for subreddit '${subreddit}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleReddit(client, iterator);
        }
    }
}

module.exports = RedditInterval;