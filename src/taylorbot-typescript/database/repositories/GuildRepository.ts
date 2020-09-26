import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import { Guild } from 'discord.js';
import * as pgPromise from 'pg-promise';

export class GuildRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async getAll(): Promise<{ guild_id: string }[]> {
        try {
            return await this.#db.any('SELECT guild_id FROM guilds.guilds;');
        }
        catch (e) {
            Log.error(`Getting all guilds: ${e}`);
            throw e;
        }
    }

    mapGuildToDatabase(guild: Guild): { guild_id: string; guild_name: string } {
        return {
            guild_id: guild.id,
            guild_name: guild.name
        };
    }

    async getPrefix(guild: Guild): Promise<{ prefix: string }> {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this.#db.one(
                `INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES ($[guild_id], $[guild_name], NULL)
                ON CONFLICT (guild_id) DO UPDATE SET
                    previous_guild_name = guilds.guilds.guild_name,
                    guild_name = excluded.guild_name
                RETURNING prefix;`,
                databaseGuild
            );
        }
        catch (e) {
            Log.error(`Getting guild prefix ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}
