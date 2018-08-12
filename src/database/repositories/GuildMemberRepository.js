'use strict';

const { Paths } = require('globalobjects');

const Log = require('../../tools/Logger.js');
const Format = require(Paths.DiscordFormatter);

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
            return await this._db.guilds.guild_members.findOne(databaseMember);
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
            return await this._db.guilds.guild_members.insert(databaseMember);
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
            return await this._db.instance.tx(async t => {
                await t.none(
                    'UPDATE guilds.guild_members SET minutes_count = minutes_count + $1 WHERE last_spoke_at > $2;',
                    [minutesToAdd, minimumLastSpoke]
                );
                await t.none([
                    'UPDATE guilds.guild_members SET ',
                    'minutes_milestone = (minutes_count-(minutes_count % $1)),',
                    'taypoints_count = taypoints_count + $2',
                    'WHERE minutes_count >= minutes_milestone + $1;'
                ].join('\n'), [minutesForReward, pointsReward]
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
            return await this._db.guilds.guild_members.update(databaseMember,
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
            return await this._db.guilds.guild_members.update(
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