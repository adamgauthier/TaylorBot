import Log = require('../../tools/Logger.js');
import * as pgPromise from 'pg-promise';
import Format = require('../../modules/DiscordFormatter.js');
import { User, Guild } from 'discord.js';

export class ProRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async proGuildExists(guild: Guild): Promise<{ guild_exists: boolean }> {
        try {
            return await this.#db.one(
                'SELECT (COUNT(*) > 0) AS guild_exists FROM guilds.pro_guilds WHERE guild_id = $[guild_id];',
                {
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting pro guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getUser(user: User): Promise<{ expires_at: Date | null; subscription_count: number } | null> {
        try {
            return await this.#db.oneOrNone(
                'SELECT * FROM users.pro_users WHERE user_id = $[user_id];',
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting pro user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async countUserProGuilds(user: User): Promise<{ count: string }> {
        try {
            return await this.#db.one(
                'SELECT COUNT(*) FROM guilds.pro_guilds WHERE pro_user_id = $[user_id];',
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Counting pro guilds for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async addUserProGuild(user: User, guild: Guild): Promise<void> {
        try {
            await this.#db.none(
                'INSERT INTO guilds.pro_guilds (guild_id, pro_user_id) VALUES ($[guild_id], $[user_id]);',
                {
                    user_id: user.id,
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Adding pro guild ${Format.guild(guild)} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async removeUserProGuild(user: User, guild: Guild): Promise<void> {
        try {
            await this.#db.none(
                'DELETE FROM guilds.pro_guilds WHERE guild_id = $[guild_id] AND pro_user_id = $[user_id];',
                {
                    user_id: user.id,
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Removing pro guild ${Format.guild(guild)} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
