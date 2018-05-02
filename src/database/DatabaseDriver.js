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
const InstagramCheckerRepository = require(GlobalPaths.InstagramCheckerRepository);
const RedditCheckerRepository = require(GlobalPaths.RedditCheckerRepository);
const YoutubeCheckerRepository = require(GlobalPaths.YoutubeCheckerRepository);
const TumblrCheckerRepository = require(GlobalPaths.TumblrCheckerRepository);
const GuildCommandRepository = require(GlobalPaths.GuildCommandRepository);

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
        this.instagramCheckers = new InstagramCheckerRepository(this._db);
        this.redditCheckers = new RedditCheckerRepository(this._db);
        this.youtubeCheckers = new YoutubeCheckerRepository(this._db);
        this.tumblrCheckers = new TumblrCheckerRepository(this._db);
        this.guildCommands = new GuildCommandRepository(this._db);
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