import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';
import { Format } from '../../modules/discord/DiscordFormatter';
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
}
