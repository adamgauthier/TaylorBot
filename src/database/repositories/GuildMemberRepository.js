'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildMemberRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guilds.guild_members.find({}, {
                fields: ['user_id', 'guild_id']
            });
        }
        catch (e) {
            Log.error(`Getting all guild members: ${e}`);
            throw e;
        }
    }

    async getAllInGuild(guild) {
        try {
            return await this._db.guilds.guild_members.find(
                {
                    'guild_id': guild.id
                },
                {
                    fields: ['user_id']
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
            return await this._db.instance.oneOrNone(
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
            return await this._db.instance.none(
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
            return await this._db.instance.any(
                `SELECT first_joined_at, user_id, rank() OVER (ORDER BY first_joined_at ASC) AS rank
                FROM guilds.guild_members
                WHERE guild_id = $[guild_id]
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
            return await this._db.instance.one(
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

    async addMinutes(minutesToAdd, minimumLastSpoke, minutesForReward, pointsReward) {
        try {
            return await this._db.instance.tx(async t => {
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
                    `UPDATE guilds.guild_members SET
                       minutes_milestone = (minute_count-(minute_count % $[minutes_for_reward])),
                       taypoint_count = taypoint_count + $[points_reward]
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
            return await this._db.instance.oneOrNone(
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
            return await this._db.instance.oneOrNone(
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
            await this._db.instance.none(
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
            await this._db.instance.none(
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
}

module.exports = GuildMemberRepository;