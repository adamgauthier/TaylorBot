'use strict';

const path = require('path');
const GlobalPaths = require(path.join(__dirname, '..', 'GlobalPaths'));

const Interval = require(GlobalPaths.Interval);
const database = require(GlobalPaths.databaseDriver);
const taylorbot = require(GlobalPaths.taylorBotClient);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const InstagramModule = require(GlobalPaths.InstagramModule);

const intervalTime = 60000;

class InstagramInterval extends Interval {
    constructor() {
        super(intervalTime, async () => {
            const instagrams = await database.getInstagrams();
            const it = instagrams.entries();
            this.checkSingleInstagram(it);
        });
    }

    async checkSingleInstagram(iterator) {
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

            if (item.code !== last_post_code) {
                Log.info(`New Instagram Post for user '${instagram_username}', ${Format.guildChannel(channel, '#name (#id), #gName (#gId)')}: ${item.code}.`);
                await taylorbot.sendEmbed(channel, InstagramModule.getRichEmbed(item, user));
                await database.updateInstagram(instagram_username, guild_id, channel_id, item.code);
            }
        }
        catch (e) {
            Log.error(`Checking Instagram Posts for user '${instagram_username}', guild ${guild_id}, channel ${channel_id}: ${e}.`);
        }
        finally {
            this.checkSingleInstagram(iterator);
        }
    }
}

module.exports = new InstagramInterval();