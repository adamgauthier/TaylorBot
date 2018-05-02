'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildUpdate extends EventHandler {
    async handler({ database }, oldGuild, newGuild) {
        if (oldGuild.name !== newGuild.name) {
            const changedAt = new Date().getTime();
            await database.guildNames.add(newGuild, changedAt);
            Log.info(`Added new guild name for ${Format.guild(newGuild)}. Old guild name was ${oldGuild.name}.`);
        }
    }
}

module.exports = GuildUpdate;