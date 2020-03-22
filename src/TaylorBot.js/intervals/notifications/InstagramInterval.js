'use strict';

const Interval = require('../Interval.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');
const InstagramModule = require('../../modules/instagram/InstagramModule.js');

class InstagramInterval extends Interval {
    constructor() {
        super({
            id: 'instagram-checker',
            intervalMs: 60000,
            enabled: false
        });
    }

    async interval(client) {
        const instagrams = await client.master.database.instagramCheckers.getAll();
        const it = instagrams.entries();
        this.checkSingleInstagram(client, it);
    }

    async checkSingleInstagram(client, iterator) {
        let current = iterator.next();
        if (current.done) return;
        current = current.value[1];
        const { guild_id, channel_id, instagram_username, last_post_code } = current;

        try {
            const guild = client.resolveGuild(guild_id);
            if (!guild) throw new Error(`Guild ID '${guild_id}' could not be resolved`);

            const channel = guild.channels.get(channel_id);
            if (!channel) throw new Error(`Channel ID '${channel_id}' could not be resolved`);

            const result = await InstagramModule.getLatestPost(instagram_username);
            const { item, user } = result;

            if (item.shortcode !== last_post_code) {
                Log.info(`New Instagram Post for user '${instagram_username}', ${Format.guildChannel(channel)}: ${item.shortcode}.`);
                await client.sendEmbed(channel, InstagramModule.getEmbed(item, user));
                await client.master.database.instagramCheckers.update(instagram_username, guild_id, channel_id, item.shortcode);
            }
        }
        catch (e) {
            Log.error(`Checking Instagram Posts for user '${instagram_username}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleInstagram(client, iterator);
        }
    }
}

module.exports = InstagramInterval;