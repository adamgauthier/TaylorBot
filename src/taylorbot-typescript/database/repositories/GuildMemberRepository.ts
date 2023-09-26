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

    getRankedForeignStat(guild: Guild, limit: number, schema: string, table: string, column: string): Promise<any[]> {
        return this._getRankedAliveForeign(guild, limit, new this.#helpers.TableName({ schema: schema, table: table }), column);
    }

    getRankedForeignStatFor(guildMember: GuildMember, schema: string, table: string, rankedColumn: string, additionalColumns: string[] = []): Promise<any> {
        return this._getRankedAliveForeignFor(guildMember, new this.#helpers.TableName({ schema: schema, table: table }), rankedColumn, additionalColumns);
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
