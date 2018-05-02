'use strict';

const massive = require('massive');

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const PostgreSQLConfig = require(GlobalPaths.PostgreSQLConfig);
const GuildRepository = require(GlobalPaths.GuildRepository);
const UserRepository = require(GlobalPaths.UserRepository);
const GuildMemberRepository = require(GlobalPaths.GuildMemberRepository);
const UsernameRepository = require(GlobalPaths.UsernameRepository);
const GuildNameRepository = require(GlobalPaths.GuildNameRepository);
const InstagramRepository = require(GlobalPaths.InstagramRepository);

class DatabaseDriver {
    async load() {
        this._db = await massive(PostgreSQLConfig, {
            'scripts': GlobalPaths.databaseScriptsPath
        });

        this.guilds = new GuildRepository(this._db);
        this.users = new UserRepository(this._db);
        this.guildMembers = new GuildMemberRepository(this._db);
        this.usernames = new UsernameRepository(this._db);
        this.guildNames = new GuildNameRepository(this._db);
        this.instagrams = new InstagramRepository(this._db);
    }

    async getReddits() {
        try {
            return await this._db.checkers.reddit_checker.find();
        }
        catch (e) {
            Log.error(`Getting Reddits: ${e}`);
            return [];
        }
    }

    async getYoutubeChannels() {
        try {
            return await this._db.checkers.youtube_checker.find();
        }
        catch (e) {
            Log.error(`Getting Youtube Channels: ${e}`);
            return [];
        }
    }

    async getTumblrs() {
        try {
            return await this._db.checkers.tumblr_checker.find();
        }
        catch (e) {
            Log.error(`Getting Tumblrs: ${e}`);
            return [];
        }
    }

    async updateTumblr(tumblrUser, guildId, channelId, lastLink) {
        try {
            return await this._db.checkers.tumblr_checker.update(
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
            return await this._db.checkers.youtube_checker.update(
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
            return await this._db.checkers.reddit_checker.update(
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

    async setGuildRoleGroup(role, group) {
        try {
            return await this._db.guild_role_groups.insert(
                {
                    'guild_id': role.guild.id,
                    'role_id': role.id,
                    'group_name': group.name
                }
            );
        }
        catch (e) {
            Log.error(`Setting guild '${Format.guild(role.guild)}' role '${Format.role(role)}' group '${group.name}': ${e}`);
            throw e;
        }
    }

    async setGuildCommandDisabled(guild, commandName, disabled) {
        try {
            return await this._db.guild_commands.upsertDisabledCommand({
                'guild_id': guild.id,
                'command_name': commandName,
                'disabled': disabled
            });
        }
        catch (e) {
            Log.error(`Upserting guild command ${Format.guild(guild)} for '${commandName}' disabled to '${disabled}': ${e}`);
            throw e;
        }
    }

    async setCommandEnabled(commandName, enabled) {
        try {
            return await this._db.commands.update(
                {
                    'name': commandName
                },
                {
                    'enabled': enabled
                }
            );
        }
        catch (e) {
            Log.error(`Setting command '${commandName}' enabled to '${enabled}': ${e}`);
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

    async setPrefix(guild, prefix) {
        try {
            return await this._db.guilds.update(
                {
                    'guild_id': guild.id
                },
                {
                    'prefix': prefix
                }
            );
        }
        catch (e) {
            Log.error(`Setting guild prefix for ${Format.guild(guild)} to '${prefix}': ${e}`);
            throw e;
        }
    }

    mapRoleToDatabase(role) {
        return {
            'role_id': role.id,
            'guild_id': role.guild.id
        };
    }

    async getSpecialRole(role) {
        const databaseRole = this.mapRoleToDatabase(role);
        try {
            return await this._db.guild_special_roles.findOne(databaseRole);
        }
        catch (e) {
            Log.error(`Getting special role ${Format.role(role)}: ${e}`);
            throw e;
        }
    }

    async setAccessibleRole(role) {
        const databaseRole = this.mapRoleToDatabase(role);
        const fields = { 'accessible': true };
        try {
            const inserted = await this._db.guild_special_roles.insert({ ...databaseRole, ...fields }, { 'onConflictIgnore': true });
            return inserted ? inserted : await this._db.guild_special_roles.update(databaseRole, fields);
        }
        catch (e) {
            Log.error(`Setting accessible special role ${Format.role(role)}: ${e}`);
            throw e;
        }
    }
}

module.exports = DatabaseDriver;