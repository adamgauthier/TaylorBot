'use strict';

const massive = require('massive');

const { GlobalPaths } = require('globalobjects');

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
const CommandRepository = require(GlobalPaths.CommandRepository);
const UserGroupRepository = require(GlobalPaths.UserGroupRepository);
const RoleGroupRepository = require(GlobalPaths.RoleGroupRepository);
const SpecialRoleRepository = require(GlobalPaths.SpecialRoleRepository);

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
        this.commands = new CommandRepository(this._db);
        this.userGroups = new UserGroupRepository(this._db);
        this.roleGroups = new RoleGroupRepository(this._db);
        this.specialRoles = new SpecialRoleRepository(this._db);
    }
}

module.exports = DatabaseDriver;