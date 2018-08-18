'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildCommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guilds.guild_commands.find();
        }
        catch (e) {
            Log.error(`Getting all guild commands: ${e}`);
            throw e;
        }
    }

    async setDisabled(guild, commandName, disabled) {
        try {
            return await this._db.guild_commands.upsertDisabledCommand({
                'guild_id': guild.id,
                'command_name': commandName,
                'disabled': disabled
            });
        }
        catch (e) {
            Log.error(`Upserting guild command ${Format.guild(guild)} for '${commandName}' disabled to '${disabled}': ${e}`);
            throw e;
        }
    }
}

module.exports = GuildCommandRepository;