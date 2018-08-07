'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const GuildMemberUnbannedLoggable = require('../modules/logging/GuildMemberUnbannedLoggable.js');

class GuildBanRemove extends EventHandler {
    constructor() {
        super(Events.GUILD_BAN_REMOVE);
    }

    async handler(client, guild, user) {
        const guildMemberUnbannedLoggable = new GuildMemberUnbannedLoggable(user, new Date());

        client.textChannelLogger.log(guild, guildMemberUnbannedLoggable);
    }
}

module.exports = GuildBanRemove;