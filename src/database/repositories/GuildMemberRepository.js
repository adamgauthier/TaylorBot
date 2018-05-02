'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

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
}

module.exports = GuildMemberRepository;