import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';
import { Format } from '../../modules/discord/DiscordFormatter';
import { BaseGuildTextChannel, ThreadChannel } from 'discord.js';
import { CachedCommand } from '../../client/registry/CachedCommand';

export class ChannelCommandRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async getIsCommandDisabledInChannel(guildTextChannel: BaseGuildTextChannel | ThreadChannel, command: CachedCommand): Promise<{ exists: boolean }> {
        try {
            return await this.#db.one(
                `SELECT EXISTS(
                    SELECT 1 FROM guilds.channel_commands
                    WHERE guild_id = $[guild_id] AND channel_id = $[channel_id] AND command_id = $[command_id]
                );`,
                {
                    guild_id: guildTextChannel.guild.id,
                    channel_id: guildTextChannel.id,
                    command_id: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Getting is disabled for ${command.name} in ${Format.guildChannel(guildTextChannel)}: ${e}`);
            throw e;
        }
    }
}
