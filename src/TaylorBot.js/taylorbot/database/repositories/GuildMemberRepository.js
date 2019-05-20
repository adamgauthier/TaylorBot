'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildMemberRepository {
    constructor(db, helpers) {
        this._db = db;
        this._helpers = helpers;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT user_id, guild_id, first_joined_at, alive FROM guilds.guild_members;');
        }
        catch (e) {
            Log.error(`Getting all guild members: ${e}`);
            throw e;
        }
    }

    async getAllInGuild(guild) {
        try {
            return await this._db.any(
                'SELECT user_id, first_joined_at, alive FROM guilds.guild_members WHERE guild_id = $[guild_id];',
                {
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting all guild members for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    mapMemberToDatabase(guildMember) {
        return {
            guild_id: guildMember.guild.id,
            user_id: guildMember.id
        };
    }

    async get(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM guilds.guild_members WHERE guild_id = $[guild_id] AND user_id = $[user_id];',
                databaseMember
            );
        }
        catch (e) {
            Log.error(`Getting guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async add(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        databaseMember.first_joined_at = guildMember.joinedAt;
        try {
            return await this._db.none(
                'INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at) VALUES ($[guild_id], $[user_id], $[first_joined_at]);',
                databaseMember
            );
        }
        catch (e) {
            Log.error(`Adding member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
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

    async reverseAliveInGuild(guild, userIds) {
        try {
            await this._db.none(
                `UPDATE guilds.guild_members
                SET alive = NOT alive
                WHERE guild_id = $[guild_id] AND user_id IN ($[user_ids:csv]);`,
                {
                    guild_id: guild.id,
                    user_ids: userIds
                }
            );
        }
        catch (e) {
            Log.error(`Reversing alive for guild ${Format.guild(guild)} and user ids ${userIds.join()}: ${e}`);
            throw e;
        }
    }

    async _setAlive(member, alive) {
        const databaseMember = this.mapMemberToDatabase(member);
        try {
            await this._db.none(
                `UPDATE guilds.guild_members
                SET alive = $[alive]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id];`,
                {
                    ...databaseMember,
                    alive
                }
            );
        }
        catch (e) {
            Log.error(`Setting guild member ${Format.member(member)} alive to ${alive}: ${e}`);
            throw e;
        }
    }

    setDead(member) {
        return this._setAlive(member, false);
    }

    setAlive(member) {
        return this._setAlive(member, true);
    }
}

module.exports = GuildMemberRepository;