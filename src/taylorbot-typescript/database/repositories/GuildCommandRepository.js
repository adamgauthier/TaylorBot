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

    async getIsGuildCommandDisabled(guild, command) {
        try {
            return await this._db.one(
                `SELECT EXISTS(
                    SELECT 1 FROM guilds.guild_commands
                    WHERE guild_id = $[guild_id] AND command_name = $[command_name] AND disabled = TRUE
                );`,
                {
                    guild_id: guild.id,
                    command_name: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Getting is disabled for ${command.name} in ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}

module.exports = GuildCommandRepository;
