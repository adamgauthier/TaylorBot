import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';
import { Format } from '../../modules/discord/DiscordFormatter';
import { BaseGuildTextChannel, ThreadChannel } from 'discord.js';
import { DatabaseCommand } from './CommandRepository';
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

    async disableCommandInChannel(guildTextChannel: BaseGuildTextChannel | ThreadChannel, command: DatabaseCommand): Promise<void> {
        try {
            await this.#db.none(
                `INSERT INTO guilds.channel_commands (guild_id, channel_id, command_id)
                VALUES ($[guild_id], $[channel_id], $[command_id]) ON CONFLICT DO NOTHING;`,
                {
                    guild_id: guildTextChannel.guild.id,
                    channel_id: guildTextChannel.id,
                    command_id: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Disabling channel command '${command.name}' for channel ${Format.channel(guildTextChannel)}: ${e}`);
            throw e;
        }
    }

    async enableCommandInChannel(guildTextChannel: BaseGuildTextChannel | ThreadChannel, command: DatabaseCommand): Promise<void> {
        try {
            await this.#db.none(
                'DELETE FROM guilds.channel_commands WHERE guild_id = $[guild_id] AND channel_id = $[channel_id] AND command_id = $[command_id];',
                {
                    guild_id: guildTextChannel.guild.id,
                    channel_id: guildTextChannel.id,
                    command_id: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Enabling channel command '${command.name}' for channel ${Format.channel(guildTextChannel)}: ${e}`);
            throw e;
        }
    }
}
