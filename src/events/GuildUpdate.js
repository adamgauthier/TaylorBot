'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class GuildUpdate extends EventHandler {
    constructor() {
        super(Events.GUILD_UPDATE);
    }

    async handler({ master }, oldGuild, newGuild) {
        if (oldGuild.name !== newGuild.name) {
            await master.database.guildNames.add(newGuild);
            Log.info(`Added new guild name for ${Format.guild(newGuild)}. Old guild name was ${oldGuild.name}.`);
        }
    }
}

module.exports = GuildUpdate;