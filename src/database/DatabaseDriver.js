'use strict';

const massive = require('massive');

const { Paths } = require('globalobjects');

const PostgreSQLConfig = require(Paths.PostgreSQLConfig);
const GuildRepository = require(Paths.GuildRepository);
const UserRepository = require(Paths.UserRepository);
const GuildMemberRepository = require(Paths.GuildMemberRepository);
const UsernameRepository = require(Paths.UsernameRepository);
const GuildNameRepository = require(Paths.GuildNameRepository);
const InstagramCheckerRepository = require(Paths.InstagramCheckerRepository);
const RedditCheckerRepository = require(Paths.RedditCheckerRepository);
const YoutubeCheckerRepository = require(Paths.YoutubeCheckerRepository);
const TumblrCheckerRepository = require(Paths.TumblrCheckerRepository);
const GuildCommandRepository = require(Paths.GuildCommandRepository);
const CommandRepository = require(Paths.CommandRepository);
const UserGroupRepository = require(Paths.UserGroupRepository);
const RoleGroupRepository = require(Paths.RoleGroupRepository);
const SpecialRoleRepository = require(Paths.SpecialRoleRepository);
const ReminderRepository = require('./repositories/ReminderRepository');
const TextChannelRepository = require('./repositories/TextChannelRepository');

class DatabaseDriver {
    async load() {
        this._db = await massive(PostgreSQLConfig, {
            'scripts': Paths.databaseScriptsPath
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
        this.reminders = new ReminderRepository(this._db);
        this.textChannels = new TextChannelRepository(this._db);
    }
}

module.exports = DatabaseDriver;