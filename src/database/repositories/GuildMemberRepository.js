'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildMemberRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guild_members.find({}, {
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
            return await this._db.guild_members.find(
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

    async exists(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            const matchingMembersCount = await this._db.guild_members.count(databaseMember);
            return matchingMembersCount > 0;
        }
        catch (e) {
            Log.error(`Checking if guild member ${Format.member(guildMember)} exists: ${e}`);
            throw e;
        }
    }

    async add(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        databaseMember.first_joined_at = guildMember.joinedTimestamp;
        try {
            return await this._db.guild_members.insert(databaseMember);
        }
        catch (e) {
            Log.error(`Adding member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async getRankedFirstJoinedAt(guildMember) {
        try {
            return await this._db.guild_members.getRankedFirstJoinedAt(
                {
                    'guild_id': guildMember.guild.id,
                    'user_id': guildMember.id
                },
                { single: true }
            );
        }
        catch (e) {
            Log.error(`Getting ranked first joined at for guild member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async addMinutes(minutesToAdd, minimumLastSpoke, minutesForReward, pointsReward) {
        try {
            return await this._db.guild_members.addMinutes({
                'minutes_to_add': minutesToAdd,
                'min_spoke_at': minimumLastSpoke,
                'minutes_for_reward': minutesForReward,
                'reward_count': pointsReward
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
            return await this._db.guild_members.update(databaseMember,
                {
                    'last_spoke_at': lastSpokeAt
                },
                {
                    'single': true
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
            return await this._db.guild_members.update(
                { 
                    ...databaseMember,
                    'first_joined_at': '9223372036854775807'
                },
                {
                    'first_joined_at': guildMember.joinedTimestamp
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Fixing Invalid Join Date for ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }
}

module.exports = GuildMemberRepository;