import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { Guild, TextChannel } from 'discord.js';

export class TextChannelRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async getAllLogChannelsInGuild(guild: Guild, type: 'member' | 'message'): Promise<{ channel_id: string }[]> {
        try {
            return await this.#db.any(
                `SELECT channel_id FROM guilds.text_channels WHERE guild_id = $[guild_id] AND is_${type}_log = TRUE;`,
                {
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting all guild ${type} log channels for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    mapChannelToDatabase(guildChannel: TextChannel): { guild_id: string; channel_id: string } {
        return {
            'guild_id': guildChannel.guild.id,
            'channel_id': guildChannel.id
        };
    }

    async get(guildChannel: TextChannel): Promise<{
        guild_id: string;
        channel_id: string;
        message_count: string;
        is_member_log: boolean;
        registered_at: Date;
        is_spam: boolean;
        is_message_log: boolean;
    } | null> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this.#db.oneOrNone(
                'SELECT * FROM guilds.text_channels WHERE guild_id = $[guild_id] AND channel_id = $[channel_id];',
                databaseChannel
            );
        }
        catch (e) {
            Log.error(`Getting text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async removeLog(guildChannel: TextChannel, type: 'member' | 'message'): Promise<void> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            await this.#db.none(
                `UPDATE guilds.text_channels SET is_${type}_log = $[is_log]
                WHERE guild_id = $[guild_id] AND channel_id = $[channel_id];`,
                {
                    is_log: false,
                    ...databaseChannel
                }
            );
        }
        catch (e) {
            Log.error(`Removing ${Format.guildChannel(guildChannel)} as a ${type} log channel: ${e}`);
            throw e;
        }
    }

    async removeAllLogsInGuild(guild: Guild): Promise<{ channel_id: string }[]> {
        try {
            return await this.#db.manyOrNone(
                `UPDATE guilds.text_channels SET is_message_log = FALSE, is_member_log = FALSE
                WHERE guild_id = $[guild_id] AND (is_message_log = TRUE OR is_member_log = TRUE)
                RETURNING channel_id;`,
                {
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Removing all log channels from ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async insertOrGetIsSpamChannelAsync(guildChannel: TextChannel): Promise<boolean> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this.#db.one(
                `INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES ($[guild_id], $[channel_id])
                ON CONFLICT (guild_id, channel_id) DO UPDATE SET
                    registered_at = guilds.text_channels.registered_at
                RETURNING is_spam;`,
                databaseChannel
            );
        }
        catch (e) {
            Log.error(`Insert or get is spam channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async upsertSpamChannel(guildChannel: TextChannel, isSpam: boolean): Promise<void> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            await this.#db.none(
                `INSERT INTO guilds.text_channels (guild_id, channel_id, is_spam) VALUES ($[guild_id], $[channel_id], $[is_spam])
                ON CONFLICT (guild_id, channel_id) DO UPDATE SET is_spam = $[is_spam];`,
                {
                    ...databaseChannel,
                    is_spam: isSpam
                }
            );
        }
        catch (e) {
            Log.error(`Upserting ${Format.guildChannel(guildChannel)} as spam channel: ${e}`);
            throw e;
        }
    }

    async upsertLogChannel(guildChannel: TextChannel, type: 'member' | 'message'): Promise<void> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            await this.#db.none(
                `INSERT INTO guilds.text_channels (guild_id, channel_id, is_${type}_log) VALUES ($[guild_id], $[channel_id], TRUE)
                ON CONFLICT (guild_id, channel_id) DO UPDATE SET is_${type}_log = TRUE;`,
                databaseChannel
            );
        }
        catch (e) {
            Log.error(`Upserting ${Format.guildChannel(guildChannel)} as a ${type} log channel: ${e}`);
            throw e;
        }
    }

    async insertChannel(guildChannel: TextChannel): Promise<void> {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            await this.#db.none(
                `INSERT INTO guilds.text_channels (guild_id, channel_id) VALUES ($[guild_id], $[channel_id])
                ON CONFLICT (guild_id, channel_id) DO NOTHING;`,
                databaseChannel
            );
        }
        catch (e) {
            Log.error(`Inserting ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }
}
