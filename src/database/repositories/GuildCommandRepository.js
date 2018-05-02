'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildCommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guild_commands.find();
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