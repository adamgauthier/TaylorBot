'use strict';

const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildDelete extends EventHandler {
    async handler(client, guild) {
        Log.info(`Guild ${Format.guild(guild)} was deleted, or client left it.`);
    }
}

module.exports = GuildDelete;