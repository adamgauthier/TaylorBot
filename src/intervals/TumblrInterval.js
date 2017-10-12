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

        try {
            const guild = taylorbot.resolveGuild(current.guildId);
            if (!guild) throw new Error(`Guild ID '${current.guildId}' could not be resolved`);

            const channel = guild.channels.get(current.channelId);
            if (!channel) throw new Error(`Channel ID '${current.channelId}' could not be resolved`);

            const result = await TumblrModule.getLatestPost(current.tumblrUser);
            const { post, blog } = result;

            if (post.short_url !== current.lastLink) {
                taylorbot.sendEmbed(channel, TumblrModule.getEmbed(post, blog));
                await database.updateTumblr(post.short_url, current.tumblrUser, current.guildId);
            }
        }
        catch (e) {
            Log.error(`Checking Tumblr Posts for user '${current.tumblrUser}' for guild ${current.guildId}: ${e}.`);
        }
        finally {
            return TumblrInterval.checkSingleTumblr(iterator);
        }
    }
}

module.exports = new TumblrInterval();