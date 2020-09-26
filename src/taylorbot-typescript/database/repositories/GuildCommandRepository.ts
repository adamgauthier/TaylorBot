import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { Guild } from 'discord.js';
import { CachedCommand } from '../../client/registry/CachedCommand';

export class GuildCommandRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async setDisabled(guild: Guild, commandName: string, disabled: boolean): Promise<{ disabled: boolean }> {
        try {
            return await this.#db.one(
                `INSERT INTO guilds.guild_commands (guild_id, command_name, disabled)
                VALUES ($[guild_id], $[command_name], $[disabled])
                ON CONFLICT (guild_id, command_name) DO UPDATE
                  SET disabled = excluded.disabled
                RETURNING disabled;`,
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

    async getIsGuildCommandDisabled(guild: Guild, command: CachedCommand): Promise<{ exists: boolean }> {
        try {
            return await this.#db.one(
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
