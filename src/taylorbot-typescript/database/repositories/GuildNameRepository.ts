import Log = require('../../tools/Logger.js');
import * as pgPromise from 'pg-promise';
import Format = require('../../modules/DiscordFormatter.js');
import { Guild } from 'discord.js';

export class GuildNameRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async add(guild: Guild): Promise<void> {
        try {
            await this.#db.none(
                'INSERT INTO guilds.guild_names (guild_id, guild_name) VALUES ($[guild_id], $[guild_name]);',
                {
                    guild_id: guild.id,
                    guild_name: guild.name
                }
            );
        }
        catch (e) {
            Log.error(`Adding guild name for ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getHistory(guild: Guild, limit: number): Promise<{ guild_name: string; changed_at: Date }[]> {
        try {
            return await this.#db.any(
                `SELECT guild_name, changed_at
                FROM guilds.guild_names
                WHERE guild_id = $[guild_id]
                ORDER BY changed_at DESC
                LIMIT $[max_rows];`,
                {
                    guild_id: guild.id,
                    max_rows: limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting guild name history for ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}
