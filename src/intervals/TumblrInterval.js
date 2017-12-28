'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const TumblrModule = require(GlobalPaths.TumblrModule);
const Log = require(GlobalPaths.Logger);

const intervalTime = 60000;

class TumblrInterval extends Interval {
    constructor() {
        super(intervalTime, TumblrInterval.intervalHandler);
    }

    static async intervalHandler() {
        const tumblrs = await database.getTumblrs();
        const it = tumblrs.entries();
        return TumblrInterval.checkSingleTumblr(it);
    }

    static async checkSingleTumblr(iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, tumblr_user, last_link } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const result = await TumblrModule.getLatestPost(tumblr_user);
            const { post, blog } = result;

            if (post.short_url !== last_link) {
                await taylorbot.sendEmbed(channel, TumblrModule.getEmbed(post, blog));
                await database.updateTumblr(tumblr_user, guild_id, channel_id, post.short_url);
            }
        }
        catch (e) {
            Log.error(`Checking Tumblr Posts for user '${tumblr_user}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            return TumblrInterval.checkSingleTumblr(iterator);
        }
    }
}

module.exports = new TumblrInterval();