'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildCommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT * FROM guilds.guild_commands;');
        }
        catch (e) {
            Log.error(`Getting all guild commands: ${e}`);
            throw e;
        }
    }

    async setDisabled(guild, commandName, disabled) {
        try {
            return await this._db.one(
                `INSERT INTO guilds.guild_commands (guild_id, command_name, disabled)
                VALUES ($[guild_id], $[command_name], $[disabled])
                ON CONFLICT (guild_id, command_name) DO UPDATE
                  SET disabled = excluded.disabled
                RETURNING *;`,
                {
                    'guild_id': guild.id,
                    'command_name': commandName,
                    'disabled': disabled
                }
            );
        }
        catch (e) {
            Log.error(`Upserting guild command ${Format.guild(guild)} for '${commandName}' disabled to '${disabled}': ${e}`);
            throw e;
        }
    }
}

module.exports = GuildCommandRepository;