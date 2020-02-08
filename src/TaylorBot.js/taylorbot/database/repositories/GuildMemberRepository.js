'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildMemberRepository {
    constructor(db, helpers) {
        this._db = db;
        this._helpers = helpers;
    }

    mapMemberToDatabase(guildMember) {
        return {
            guild_id: guildMember.guild.id,
            user_id: guildMember.id
        };
    }

    async getRankedFirstJoinedAt(guild, limit) {
        try {
            return await this._db.any(
                `SELECT first_joined_at, user_id, rank
                FROM (
                   SELECT
                        first_joined_at,
                        user_id,
                        alive,
                        rank() OVER (ORDER BY first_joined_at ASC) AS rank
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id] AND first_joined_at IS NOT NULL
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

    async getRankedFirstJoinedAtFor(guildMember) {
        try {
            return await this._db.oneOrNone(
                `SELECT ranked.first_joined_at, ranked.rank
                FROM (
                   SELECT
                       first_joined_at,
                       user_id,
                       rank() OVER (ORDER BY first_joined_at ASC) AS rank
                   FROM guilds.guild_members
                   WHERE guild_id = $[guild_id] AND first_joined_at IS NOT NULL
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

    async _getRankedUsers(guild, limit, column) {
        try {
            return await this._db.any(
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

    async _getRankedAliveFor(guildMember, column) {
        try {
            return await this._db.one(
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

    async _getRankedAliveForeign(guild, limit, tableName, column) {
        try {
            return await this._db.any(
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

    async _getRankedAliveForeignFor(guildMember, tableName, rankedColumn, columns) {
        try {
            return await this._db.oneOrNone(
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

    getRankedMessages(guild, limit) {
        return this._getRankedUsers(guild, limit, 'message_count');
    }

    getRankedMessagesFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'message_count');
    }

    getRankedWords(guild, limit) {
        return this._getRankedUsers(guild, limit, 'word_count');
    }

    getRankedWordsFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'word_count');
    }

    getRankedMinutes(guild, limit) {
        return this._getRankedUsers(guild, limit, 'minute_count');
    }

    getRankedMinutesFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'minute_count');
    }

    getRankedTaypoints(guild, limit) {
        return this._getRankedUsers(guild, limit, 'taypoint_count');
    }

    getRankedTaypointsFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'taypoint_count');
    }

    getRankedForeignStat(guild, limit, schema, table, column) {
        return this._getRankedAliveForeign(guild, limit, new this._helpers.TableName(table, schema), column);
    }

    getRankedForeignStatFor(guildMember, schema, table, rankedColumn, additionalColumns = []) {
        return this._getRankedAliveForeignFor(guildMember, new this._helpers.TableName(table, schema), rankedColumn, additionalColumns);
    }

    async fixInvalidJoinDate(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.oneOrNone(
                `UPDATE guilds.guild_members SET first_joined_at = $[first_joined_at]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id] AND first_joined_at IS NULL
                RETURNING *;`,
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

    async addOrUpdateMemberAsync(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            const { experience } = await this._db.one(
                `INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES ($[guild_id], $[user_id], $[first_joined_at])
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END,
                    experience = guild_members.experience + 1
                RETURNING experience;`,
                {
                    'first_joined_at': guildMember.joinedAt,
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

module.exports = GuildMemberRepository;
