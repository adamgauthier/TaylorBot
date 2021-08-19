import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { BaseGuildTextChannel, ThreadChannel } from 'discord.js';

export class TextChannelRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    mapChannelToDatabase(guildChannel: BaseGuildTextChannel | ThreadChannel): { guild_id: string; channel_id: string } {
        return {
            'guild_id': guildChannel.guild.id,
            'channel_id': guildChannel.id
        };
    }

    async get(guildChannel: BaseGuildTextChannel | ThreadChannel): Promise<{
        guild_id: string;
        channel_id: string;
        message_count: string;
        registered_at: Date;
        is_spam: boolean;
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

    async insertOrGetIsSpamChannelAsync(guildChannel: BaseGuildTextChannel | ThreadChannel): Promise<boolean> {
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

    async upsertSpamChannel(guildChannel: BaseGuildTextChannel | ThreadChannel, isSpam: boolean): Promise<void> {
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
}
