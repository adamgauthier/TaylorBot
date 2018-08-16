'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require(Paths.DiscordFormatter);

class GuildDelete extends EventHandler {
    constructor() {
        super(Events.GUILD_DELETE);
    }

    async handler(client, guild) {
        Log.info(`Guild ${Format.guild(guild)} was deleted, or client left it.`);
    }
}

module.exports = GuildDelete;