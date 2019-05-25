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

        Log.info('Caching new guilds and users...');
        await this.syncRegistry(client);
        Log.info('New guilds and users cached!');
    }

    async syncRegistry(client) {
        const { registry } = client.master;

        for (const guild of client.guilds.values()) {
            if (!registry.guilds.has(guild.id)) {
                Log.info(`Adding new guild ${Format.guild(guild)}.`);
                await registry.guilds.cacheGuild({ guild_id: guild.id, prefix: '!' });
            }

            const members = await guild.members.fetch();

            for (const member of members.values()) {
                const { user } = member;
                if (!registry.users.has(member.id)) {
                    Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
                    await registry.users.cacheUser({ user_id: member.id, ignoreUntil: new Date() });
                }
            }
        }
    }
}

module.exports = ClientReady;