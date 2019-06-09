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
    }
}

module.exports = GuildCreate;
