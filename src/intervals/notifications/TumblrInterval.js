'use strict';

const Interval = require('../Interval.js');
const TumblrModule = require('../../modules/tumblr/TumblrModule.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class TumblrInterval extends Interval {
    constructor() {
        super({
            id: 'tumblr-checker',
            intervalMs: 60 * 1000
        });
    }

    async interval(client) {
        const tumblrs = await client.master.database.tumblrCheckers.getAll();
        const it = tumblrs.entries();
        TumblrInterval.checkSingleTumblr(client, it);
    }

    static async checkSingleTumblr(client, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, tumblr_user, last_link } = current;

        try {
            const guild = client.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const result = await TumblrModule.getLatestPost(tumblr_user, 1);
            const { post, blog } = result;

            if (post.short_url !== last_link) {
                Log.info(`New Tumblr Post for user '${tumblr_user}', ${Format.guildChannel(channel)}: ${post.short_url}.`);
                await client.sendEmbed(channel, TumblrModule.getEmbed(post, blog));
                await client.master.database.tumblrCheckers.update(tumblr_user, guild_id, channel_id, post.short_url);
            }
        }
        catch (e) {
            Log.error(`Checking Tumblr Posts for user '${tumblr_user}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            TumblrInterval.checkSingleTumblr(client, iterator);
        }
    }
}

module.exports = TumblrInterval;