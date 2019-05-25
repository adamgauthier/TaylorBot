'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class GuildCreate extends EventHandler {
    constructor() {
        super(Events.GUILD_CREATE);
    }

    async handler(client, guild) {
        const { registry } = client.master;

        if (!registry.guilds.has(guild.id)) {
            Log.info(`Adding new guild ${Format.guild(guild)}.`);
            await registry.guilds.cacheGuild({ guild_id: guild.id, prefix: '!' });
        }

        const members = await guild.members.fetch();

        for (const member of members.values()) {
            const { user } = member;
            if (!registry.users.has(member.id)) {
                Log.info(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                await registry.users.cacheUser({ user_id: member.id, ignoreUntil: new Date() });
            }
        }
    }
}

module.exports = GuildCreate;