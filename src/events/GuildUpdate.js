'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildUpdate extends EventHandler {
    constructor() {
        super(Events.GUILD_UPDATE);
    }

    async handler({ master }, oldGuild, newGuild) {
        if (oldGuild.name !== newGuild.name) {
            const changedAt = Date.now();
            await master.database.guildNames.add(newGuild, changedAt);
            Log.info(`Added new guild name for ${Format.guild(newGuild)}. Old guild name was ${oldGuild.name}.`);
        }
    }
}

module.exports = GuildUpdate;