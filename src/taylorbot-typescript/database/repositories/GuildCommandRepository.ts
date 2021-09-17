import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { Guild } from 'discord.js';
import { CachedCommand } from '../../client/registry/CachedCommand';

export class GuildCommandRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
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
