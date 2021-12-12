import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import pgPromise = require('pg-promise');
import { Guild, User } from 'discord.js';

export class BirthdayAttributeRepository {
    #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async get(user: User): Promise<{ birthday: string; is_private: boolean } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT birthday::text, is_private FROM attributes.birthdays WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting birthday attribute for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getZodiac(user: User): Promise<{ zodiac: string } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT zodiac(birthday) FROM attributes.birthdays WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting zodiac birthday attribute for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async set(user: User, birthdayString: string, isPrivate: boolean): Promise<{ user_id: string; birthday_string: string; is_private: boolean }> {
        try {
            return await this.#db.one(
                `INSERT INTO attributes.birthdays (user_id, birthday, is_private)
                VALUES ($[user_id], date $[birthday_string], $[is_private])
                ON CONFLICT (user_id) DO UPDATE SET
                    birthday = excluded.birthday,
                    is_private = excluded.is_private
                RETURNING user_id, birthday::text, is_private;`,
                {
                    user_id: user.id,
                    birthday_string: birthdayString,
                    is_private: !!isPrivate
                }
            );
        }
        catch (e) {
            Log.error(`Setting birthday attribute to '${birthdayString}', private ${isPrivate} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async clear(user: User): Promise<void> {
        try {
            await this.#db.any(
                `DELETE FROM attributes.birthdays WHERE user_id = $[user_id];`,
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Clearing birthday attribute for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getUpcomingInGuild(guild: Guild, limit: number): Promise<{ user_id: string; next_birthday: string }[]> {
        try {
            return await this.#db.any(
                `SELECT user_id,
                CASE
                    WHEN normalized_birthday < CURRENT_DATE
                    THEN (normalized_birthday + INTERVAL '1 YEAR')::text
                    ELSE normalized_birthday::text
                END
                AS next_birthday
                FROM attributes.birthdays, make_date(
                    date_part('year', CURRENT_DATE)::int,
                    date_part('month', birthday)::int,
                    CASE
                        WHEN date_part('month', birthday)::int = 2 AND date_part('day', birthday)::int = 29
                        THEN 28
                        ELSE date_part('day', birthday)::int
                    END
                ) AS normalized_birthday
                WHERE is_private = FALSE
                AND user_id IN (
                   SELECT user_id
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id] AND alive = TRUE
                )
                ORDER BY next_birthday
                LIMIT $[limit];`,
                {
                    guild_id: guild.id,
                    limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting ${limit} upcoming birthdays for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }
}
