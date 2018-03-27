'use strict';

const massive = require('massive');

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const PostgreSQLConfig = require(GlobalPaths.PostgreSQLConfig);

class DatabaseDriver {
    async load() {
        this._db = await massive(PostgreSQLConfig, { 'scripts': __dirname });
    }

    async getAllGuilds() {
        try {
            return await this._db.guilds.find();
        }
        catch (e) {
            Log.error(`Getting all guilds: ${e}`);
            throw e;
        }
    }

    mapGuildToDatabase(guild) {
        return {
            'guild_id': guild.id
        };
    }

    async getGuild(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.findOne(databaseGuild);
        }
        catch (e) {
            Log.error(`Getting guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async addGuild(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.insert(databaseGuild);
        }
        catch (e) {
            Log.error(`Adding guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getAllUsers() {
        try {
            return await this._db.users.find({}, {
                fields: ['user_id', 'last_command', 'last_answered', 'ignore_until']
            });
        }
        catch (e) {
            Log.error(`Getting all users: ${e}`);
            throw e;
        }
    }

    mapUserToDatabase(user) {
        return {
            'user_id': user.id
        };
    }

    async getUser(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.findOne(databaseUser);
        }
        catch (e) {
            Log.error(`Getting user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async addUser(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.insert(databaseUser);
        }
        catch (e) {
            Log.error(`Adding user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getAllGuildMembers() {
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

    async getAllGuildMembersInGuild(guild) {
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

    async doesGuildMemberExist(guildMember) {
        const databaseMember = { 'guild_id': guildMember.guild.id, 'user_id': guildMember.id };
        try {
            const matchingMembersCount = await this._db.guild_members.count(databaseMember);
            return matchingMembersCount > 0;
        }
        catch (e) {
            Log.error(`Checking if guild member ${Format.member(guildMember)} exists: ${e}`);
            throw e;
        }
    }

    mapMemberToDatabase(guildMember) {
        return {
            'guild_id': guildMember.guild.id,
            'user_id': guildMember.id,
            'first_joined_at': guildMember.joinedTimestamp
        }
    }

    async addGuildMember(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.guild_members.insert(databaseMember);
        }
        catch (e) {
            Log.error(`Adding member ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }

    async getLatestUsernames() {
        try {
            return await this._db.usernames.getLatestUsernames();
        }
        catch (e) {
            Log.error(`Getting all usernames: ${e}`);
            throw e;
        }
    }

    async getLatestUsername(user) {
        try {
            return await this._db.usernames.getLatestUsername(
                {
                    'user_id': user.id
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Getting latest username for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    mapUserToUsernameDatabase(user, changedAt) {
        return {
            'user_id': user.id,
            'username': user.username,
            'changed_at': changedAt
        }
    }

    async addUsername(user, changedAt) {
        const databaseUsername = this.mapUserToUsernameDatabase(user, changedAt);
        try {
            return await this._db.usernames.insert(databaseUsername);
        }
        catch (e) {
            Log.error(`Adding username for ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async getInstagrams() {
        try {
            return await this._db.instagram_checker.find();
        }
        catch (e) {
            Log.error(`Getting Instagrams: ${e}`);
            return [];
        }
    }

    async getReddits() {
        try {
            return await this._db.reddit_checker.find();
        }
        catch (e) {
            Log.error(`Getting Reddits: ${e}`);
            return [];
        }
    }

    async getYoutubeChannels() {
        try {
            return await this._db.youtube_checker.find();
        }
        catch (e) {
            Log.error(`Getting Youtube Channels: ${e}`);
            return [];
        }
    }

    async getTumblrs() {
        try {
            return await this._db.tumblr_checker.find();
        }
        catch (e) {
            Log.error(`Getting Tumblrs: ${e}`);
            return [];
        }
    }

    async updateTumblr(tumblrUser, guildId, channelId, lastLink) {
        try {
            return await this._db.tumblr_checker.update(
                {
                    'tumblr_user': tumblrUser,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_link': lastLink
                }
            );
        }
        catch (e) {
            Log.error(`Updating Tumblr for guild ${guildId}, channel ${channelId}, user ${tumblrUser}: ${e}`);
            throw e;
        }
    }

    async updateYoutube(playlistId, guildId, channelId, lastVideoId) {
        try {
            return await this._db.youtube_checker.update(
                {
                    'playlist_id': playlistId,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_video_id': lastVideoId
                }
            );
        }
        catch (e) {
            Log.error(`Updating Youtube for guild ${guildId}, channel ${channelId}, playlistId ${playlistId}: ${e}`);
            throw e;
        }
    }

    async updateReddit(subreddit, guildId, channelId, lastLink, lastCreated) {
        try {
            return await this._db.reddit_checker.update(
                {
                    'subreddit': subreddit,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_post_id': lastLink,
                    'last_created': lastCreated
                }
            );
        }
        catch (e) {
            Log.error(`Updating Reddit for guild ${guildId}, channel ${channelId}, subreddit ${subreddit}: ${e}`);
            throw e;
        }
    }

    async updateInstagram(instagramUsername, guildId, channelId, lastCode) {
        try {
            return await this._db.instagram_checker.update(
                {
                    'instagram_username': instagramUsername,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_post_code': lastCode
                }
            );
        }
        catch (e) {
            Log.error(`Updating Instagram for guild ${guildId}, channel ${channelId}, username ${instagramUsername}: ${e}`);
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

    async updateLastSpoke(user, guild, lastSpokeAt) {
        try {
            return await this._db.guild_members.update(
                {
                    'guild_id': guild.id,
                    'user_id': user.id
                },
                {
                    'last_spoke_at': lastSpokeAt
                }
            );
        }
        catch (e) {
            Log.error(`Updating Last Spoke for ${Format.user(user)}, ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getAllGuildCommands() {
        try {
            return await this._db.guild_commands.find();
        }
        catch (e) {
            Log.error(`Getting all guild commands: ${e}`);
            throw e;
        }
    }

    async getAllCommands() {
        try {
            return await this._db.commands.find();
        }
        catch (e) {
            Log.error(`Getting all commands: ${e}`);
            throw e;
        }
    }

    async addCommands(databaseCommands) {
        try {
            return await this._db.commands.insert(databaseCommands);
        }
        catch (e) {
            Log.error(`Adding commands: ${e}`);
            throw e;
        }
    }

    async getAllUserGroups() {
        try {
            return await this._db.user_groups.find();
        }
        catch (e) {
            Log.error(`Getting all user groups: ${e}`);
            throw e;
        }
    }

    async addUserGroups(userGroups) {
        try {
            return await this._db.user_groups.insert(userGroups);
        }
        catch (e) {
            Log.error(`Adding user groups: ${e}`);
            throw e;
        }
    }

    async getAllGuildRoleGroups() {
        try {
            return await this._db.guild_role_groups.find();
        }
        catch (e) {
            Log.error(`Getting all guild role groups: ${e}`);
            throw e;
        }
    }
}

module.exports = DatabaseDriver;