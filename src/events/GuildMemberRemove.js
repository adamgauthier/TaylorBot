'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const GuildMemberLeftLoggable = require('../modules/logging/GuildMemberLeftLoggable.js');

class GuildMemberRemove extends EventHandler {
    constructor() {
        super(Events.GUILD_MEMBER_REMOVE);
    }

    async handler(client, member) {
        const guildMemberLeftLoggable = new GuildMemberLeftLoggable(member, new Date());

        client.textChannelLogger.log(member.guild, guildMemberLeftLoggable);
    }
}

module.exports = GuildMemberRemove;