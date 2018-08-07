'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const GuildMemberBannedLoggable = require('../modules/logging/GuildMemberBannedLoggable.js');

class GuildBanAdd extends EventHandler {
    constructor() {
        super(Events.GUILD_BAN_ADD);
    }

    async handler(client, guild, user) {
        const guildMemberBannedLoggable = new GuildMemberBannedLoggable(user, new Date());

        client.textChannelLogger.log(guild, guildMemberBannedLoggable);
    }
}

module.exports = GuildBanAdd;