import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { Guild, GuildMember } from 'discord.js';

export class GuildMemberRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #helpers: pgPromise.IHelpers;

    constructor(db: pgPromise.IDatabase<unknown>, helpers: pgPromise.IHelpers) {
        this.#db = db;
        this.#helpers = helpers;
    }

    mapMemberToDatabase(guildMember: GuildMember): { guild_id: string; user_id: string } {
        return {
            guild_id: guildMember.guild.id,
            user_id: guildMember.id
        };
    }

    async getRankedFirstJoinedAt(guild: Guild, limit: number): Promise<{ first_joined_at: Date; user_id: string; rank: string }[]> {
        try {
            return await this.#db.any(
                `SELECT first_joined_at, user_id, rank
                FROM (
                   SELECT
                        first_joined_at,
                        user_id,
                        alive,
                        rank() OVER (ORDER BY first_joined_at ASC) AS rank
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id]
                ) AS ranked
                WHERE alive = TRUE
                LIMIT $[limit];`,
                {
                    guild_id: guild.id,
                    limit: limit
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked first joined at for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getRankedFirstJoinedAtFor(guildMember: GuildMember): Promise<{ first_joined_at: Date; rank: string } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT ranked.first_joined_at, ranked.rank
                FROM (
                   SELECT
                       first_joined_at,
                       user_id,
                       rank() OVER (ORDER BY first_joined_at ASC) AS rank
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id]
                ) AS ranked
                WHERE ranked.user_id = $[user_id];`,
                {
                    guild_id: guildMember.guild.id,
                    user_id: guildMember.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked first joined at for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async _getMemberAttribute(guildMember: GuildMember, column: string): Promise<any> {
        try {
            return await this.#db.one(
                `SELECT $[column~]
                FROM guilds.guild_members
                WHERE guild_id = $[guild_id] AND user_id = $[user_id];`,
                {
                    guild_id: guildMember.guild.id,
                    user_id: guildMember.id,
                    column
                }
            );
        }
        catch (e) {
            Log.error(`Getting attribute '${column}' for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async _getRankedUsers(guild: Guild, limit: number, column: string): Promise<any[]> {
        try {
            return await this.#db.any(
                `SELECT $[column~], gm.user_id, rank() OVER (ORDER BY $[column~] DESC) AS rank
                FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
                WHERE gm.guild_id = $[guild_id] AND gm.alive = TRUE AND u.is_bot = FALSE
                LIMIT $[limit];`,
                {
                    guild_id: guild.id,
                    limit: limit,
                    column
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked alive '${column}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async _getRankedAliveFor(guildMember: GuildMember, column: string): Promise<any> {
        try {
            return await this.#db.one(
                `SELECT $[column~], rank FROM (
                    SELECT $[column~], gm.user_id, rank() OVER (ORDER BY $[column~] DESC) AS rank
                    FROM guilds.guild_members AS gm JOIN users.users AS u ON u.user_id = gm.user_id
                    WHERE gm.guild_id = $[guild_id] AND gm.alive = TRUE ${guildMember.user.bot ? '' : 'AND u.is_bot = FALSE'}
                ) AS ranked
                WHERE user_id = $[user_id];`,
                {
                    guild_id: guildMember.guild.id,
                    user_id: guildMember.id,
                    column
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked alive '${column}' for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async _getRankedAliveForeign(guild: Guild, limit: number, tableName: pgPromise.TableName, column: string): Promise<any[]> {
        try {
            return await this.#db.any(
                `SELECT f.$[column~], gm.user_id, rank() OVER (ORDER BY f.$[column~] DESC) AS rank
                FROM guilds.guild_members AS gm
                    JOIN users.users AS u ON u.user_id = gm.user_id
                    JOIN $[table] AS f ON gm.user_id = f.user_id
                WHERE gm.guild_id = $[guild_id] AND gm.alive = TRUE AND u.is_bot = FALSE
                LIMIT $[limit];`,
                {
                    guild_id: guild.id,
                    limit: limit,
                    column,
                    table: tableName
                }
            );
        }
        catch (e) {
            Log.error(`Getting foreign ranked alive '${tableName}.${column}' for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async _getRankedAliveForeignFor(guildMember: GuildMember, tableName: pgPromise.TableName, rankedColumn: string, columns: string[]): Promise<any> {
        try {
            return await this.#db.oneOrNone(
                `SELECT $[columns~], rank FROM (
                    SELECT $[columns~], gm.user_id, rank() OVER (ORDER BY f.$[ranked_column~] DESC) AS rank
                    FROM guilds.guild_members AS gm
                        JOIN users.users AS u ON u.user_id = gm.user_id
                        JOIN $[table] AS f ON gm.user_id = f.user_id
                    WHERE gm.guild_id = $[guild_id] AND gm.alive = TRUE ${guildMember.user.bot ? '' : 'AND u.is_bot = FALSE'}
                ) AS ranked
                WHERE user_id = $[user_id];`,
                {
                    guild_id: guildMember.guild.id,
                    user_id: guildMember.id,
                    ranked_column: rankedColumn,
                    columns: [rankedColumn, ...columns],
                    table: tableName
                }
            );
        }
        catch (e) {
            Log.error(`Getting foreign ranked alive '${tableName}.${rankedColumn}' with additional columns '${columns.join()}' for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    getRankedMessages(guild: Guild, limit: number): Promise<{ message_count: string; user_id: string; rank: string }[]> {
        return this._getRankedUsers(guild, limit, 'message_count');
    }

    getMessagesFor(guildMember: GuildMember): Promise<{ message_count: string; }> {
        return this._getMemberAttribute(guildMember, 'message_count');
    }

    getRankedMessagesFor(guildMember: GuildMember): Promise<{ message_count: string; rank: string }> {
        return this._getRankedAliveFor(guildMember, 'message_count');
    }

    getRankedWords(guild: Guild, limit: number): Promise<{ word_count: string; user_id: string; rank: string }[]> {
        return this._getRankedUsers(guild, limit, 'word_count');
    }

    getWordsFor(guildMember: GuildMember): Promise<{ word_count: string; }> {
        return this._getMemberAttribute(guildMember, 'word_count');
    }

    getRankedWordsFor(guildMember: GuildMember): Promise<{ word_count: string; rank: string }> {
        return this._getRankedAliveFor(guildMember, 'word_count');
    }

    getRankedMinutes(guild: Guild, limit: number): Promise<{ minute_count: string; user_id: string; rank: string }[]> {
        return this._getRankedUsers(guild, limit, 'minute_count');
    }

    getMinutesFor(guildMember: GuildMember): Promise<{ minute_count: string }> {
        return this._getMemberAttribute(guildMember, 'minute_count');
    }

    getRankedMinutesFor(guildMember: GuildMember): Promise<{ minute_count: string; rank: string }> {
        return this._getRankedAliveFor(guildMember, 'minute_count');
    }

    getRankedForeignStat(guild: Guild, limit: number, schema: string, table: string, column: string): Promise<any[]> {
        return this._getRankedAliveForeign(guild, limit, new this.#helpers.TableName({ schema: schema, table: table }), column);
    }

    getRankedForeignStatFor(guildMember: GuildMember, schema: string, table: string, rankedColumn: string, additionalColumns: string[] = []): Promise<any> {
        return this._getRankedAliveForeignFor(guildMember, new this.#helpers.TableName({ schema: schema, table: table }), rankedColumn, additionalColumns);
    }

    async fixInvalidJoinDate(guildMember: GuildMember): Promise<void> {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            await this.#db.none(
                `UPDATE guilds.guild_members SET first_joined_at = $[first_joined_at]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id] AND first_joined_at IS NULL;`,
                {
                    'first_joined_at': guildMember.joinedAt,
                    ...databaseMember
                }
            );
        }
        catch (e) {
            Log.error(`Fixing Invalid Join Date for ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async addOrUpdateMemberAsync(guildMember: GuildMember, lastSpokeAt: Date | null): Promise<boolean> {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            const { experience } = await this.#db.one(
                `INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at, last_spoke_at) VALUES ($[guild_id], $[user_id], $[first_joined_at], $[last_spoke_at])
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END,
                    experience = guild_members.experience + 1,
                    last_spoke_at = CASE
                        WHEN excluded.last_spoke_at IS NULL
                        THEN guild_members.last_spoke_at
                        ELSE excluded.last_spoke_at
                    END
                RETURNING experience;`,
                {
                    first_joined_at: guildMember.joinedAt,
                    last_spoke_at: lastSpokeAt,
                    ...databaseMember
                }
            );

            return experience === '0';
        }
        catch (e) {
            Log.error(`Adding or updating ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }
}
