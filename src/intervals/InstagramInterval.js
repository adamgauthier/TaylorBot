'use strict';

const { GlobalPaths } = require('globalobjects');

const Interval = require(GlobalPaths.Interval);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const InstagramModule = require(GlobalPaths.InstagramModule);

const intervalTime = 60000;

class InstagramInterval extends Interval {
    constructor() {
        super(intervalTime, false);
    }

    async interval(taylorbot) {
        const instagrams = await taylorbot.database.instagramCheckers.getAll();
        const it = instagrams.entries();
        this.checkSingleInstagram(taylorbot, it);
    }

    async checkSingleInstagram(taylorbot, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, instagram_username, last_post_code } = current;

        try {
            const guild = taylorbot.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const result = await InstagramModule.getLatestPost(instagram_username);
            const { item, user } = result;

            if (item.shortcode !== last_post_code) {
                Log.info(`New Instagram Post for user '${instagram_username}', ${Format.guildChannel(channel, '#name (#id), #gName (#gId)')}: ${item.shortcode}.`);
                await taylorbot.sendEmbed(channel, InstagramModule.getEmbed(item, user));
                await taylorbot.database.instagramCheckers.update(instagram_username, guild_id, channel_id, item.shortcode);
            }
        }
        catch (e) {
            Log.error(`Checking Instagram Posts for user '${instagram_username}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleInstagram(taylorbot, iterator);
        }
    }
}

module.exports = InstagramInterval;