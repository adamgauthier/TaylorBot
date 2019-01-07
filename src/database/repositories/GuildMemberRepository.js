'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildMemberRepository {
    constructor(db) {
        this._db = db;
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
                    'guild_id': guild.id
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
            'guild_id': guildMember.guild.id,
            'user_id': guildMember.id
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
        databaseMember.first_joined_at = guildMember.joinedTimestamp;
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
                   WHERE guild_id = $[guild_id]
                ) AS ranked
                WHERE alive = TRUE
                LIMIT $[limit];`,
                {
                    'guild_id': guild.id,
                    'limit': limit
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
            return await this._db.one(
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
                    'guild_id': guildMember.guild.id,
                    'user_id': guildMember.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked first joined at for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async _getRankedAlive(guild, limit, column) {
        try {
            return await this._db.any(
                `SELECT $[column~], u.user_id, rank() OVER (ORDER BY $[column~] DESC) AS rank
                FROM guilds.guild_members AS gm JOIN users.users AS u ON gm.user_id = u.user_id
                WHERE guild_id = $[guild_id] AND alive = TRUE
                LIMIT $[limit];`,
                {
                    'guild_id': guild.id,
                    'limit': limit,
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
                    SELECT $[column~], u.user_id, rank() OVER (ORDER BY $[column~] DESC) AS rank
                    FROM guilds.guild_members AS gm JOIN users.users AS u ON gm.user_id = u.user_id
                    WHERE guild_id = $[guild_id] AND alive = TRUE
                ) AS ranked
                WHERE user_id = $[user_id];`,
                {
                    'guild_id': guildMember.guild.id,
                    'user_id': guildMember.id,
                    column
                }
            );
        }
        catch (e) {
            Log.error(`Getting ranked alive '${column}' for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    getRankedMessages(guild, limit) {
        return this._getRankedAlive(guild, limit, 'message_count');
    }

    getRankedMessagesFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'message_count');
    }

    getRankedWords(guild, limit) {
        return this._getRankedAlive(guild, limit, 'word_count');
    }

    getRankedWordsFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'word_count');
    }

    getRankedMinutes(guild, limit) {
        return this._getRankedAlive(guild, limit, 'minute_count');
    }

    getRankedMinutesFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'minute_count');
    }

    getRankedTaypoints(guild, limit) {
        return this._getRankedAlive(guild, limit, 'taypoint_count');
    }

    getRankedTaypointsFor(guildMember) {
        return this._getRankedAliveFor(guildMember, 'taypoint_count');
    }

    async addMinutes(minutesToAdd, minimumLastSpoke, minutesForReward, pointsReward) {
        try {
            return await this._db.tx(async t => {
                await t.none(
                    `UPDATE guilds.guild_members
                    SET minute_count = minute_count + $[minutes_to_add]
                    WHERE last_spoke_at > $[minimum_last_spoke];`,
                    {
                        minutes_to_add: minutesToAdd,
                        minimum_last_spoke: minimumLastSpoke
                    }
                );
                await t.none(
                    `UPDATE users.users SET
                       taypoint_count = taypoint_count + $[points_reward]
                    WHERE user_id IN (
                        SELECT user_id FROM guilds.guild_members
                        WHERE minute_count >= minutes_milestone + $[minutes_for_reward]
                    );`,
                    {
                        minutes_for_reward: minutesForReward,
                        points_reward: pointsReward
                    }
                );
                await t.none(
                    `UPDATE guilds.guild_members SET
                       minutes_milestone = (minute_count - (minute_count % $[minutes_for_reward])),
                       experience = experience + $[points_reward]
                    WHERE minute_count >= minutes_milestone + $[minutes_for_reward];`,
                    {
                        minutes_for_reward: minutesForReward,
                        points_reward: pointsReward
                    }
                );
            });
        }
        catch (e) {
            Log.error(`Adding minutes: ${e}`);
            throw e;
        }
    }

    async updateLastSpoke(guildMember, lastSpokeAt) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.oneOrNone(
                `UPDATE guilds.guild_members SET last_spoke_at = $[last_spoke_at]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id]
                RETURNING *;`,
                {
                    'last_spoke_at': lastSpokeAt,
                    ...databaseMember
                }
            );
        }
        catch (e) {
            Log.error(`Updating Last Spoke for ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async fixInvalidJoinDate(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.oneOrNone(
                `UPDATE guilds.guild_members SET first_joined_at = $[first_joined_at]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id] AND first_joined_at = 9223372036854775807
                RETURNING *;`,
                {
                    'first_joined_at': guildMember.joinedTimestamp,
                    ...databaseMember
                }
            );
        }
        catch (e) {
            Log.error(`Fixing Invalid Join Date for ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async addMessagesAndWords(guildMember, messagesToAdd, wordsToAdd) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            await this._db.none(
                `UPDATE guilds.guild_members SET
                   message_count = message_count + $[messages_to_add],
                   word_count = word_count + $[words_to_add]
                WHERE guild_id = $[guild_id] AND user_id = $[user_id];`,
                {
                    ...databaseMember,
                    messages_to_add: messagesToAdd,
                    words_to_add: wordsToAdd,
                }
            );
        }
        catch (e) {
            Log.error(`Adding ${messagesToAdd} messages and ${wordsToAdd} words for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async removeMessagesAndWords(guildMember, messagesToRemove, wordsToRemove) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            await this._db.none(
                `UPDATE guilds.guild_members SET
                   message_count = GREATEST(0, message_count - $[message_to_remove]),
                   word_count = GREATEST(0, word_count - $[words_to_remove])
                WHERE guild_id = $[guild_id] AND user_id = $[user_id];`,
                {
                    ...databaseMember,
                    message_to_remove: messagesToRemove,
                    words_to_remove: wordsToRemove
                }
            );
        }
        catch (e) {
            Log.error(`Removing ${messagesToRemove} messages and ${wordsToRemove} words from guild member ${Format.member(guildMember)}: ${e}`);
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