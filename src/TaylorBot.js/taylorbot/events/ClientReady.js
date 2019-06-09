'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class ClientReady extends EventHandler {
    constructor() {
        super(Events.CLIENT_READY);
    }

    async handler(client) {
        Log.info('Client is ready!');

        client.intervalRunner.startAll();
        Log.info('Intervals started!');

        Log.info('Caching new guilds...');
        await this.syncRegistry(client);
        Log.info('New guilds cached!');
    }

    async syncRegistry(client) {
        const { registry } = client.master;

        for (const guild of client.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.info(`Adding new guild ${Format.guild(guild)}.`);
                await registry.guilds.cacheGuild({ guild_id: guild.id, prefix: '!' });
            }
        }
    }
}

module.exports = ClientReady;
