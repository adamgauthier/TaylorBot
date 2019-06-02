'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class GuildMemberAdd extends EventHandler {
    constructor() {
        super(Events.GUILD_MEMBER_ADD);
    }

    async handler(client, member) {
        const { user, guild } = member;
        const { registry } = client.master;

        if (!registry.users.has(member.id)) {
            Log.warn(`Found new user ${Format.user(user)} in guild ${Format.guild(guild)}.`);
            await registry.users.cacheUser({ user_id: member.id, ignoreUntil: new Date() });
        }
    }
}

module.exports = GuildMemberAdd;