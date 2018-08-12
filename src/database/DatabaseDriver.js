'use strict';

const massive = require('massive');

const PostgreSQLConfig = require('../config/postgresql.json');

const GuildRepository = require('./repositories/GuildRepository');
const UserRepository = require('./repositories/UserRepository');
const GuildMemberRepository = require('./repositories/GuildMemberRepository');
const UsernameRepository = require('./repositories/UsernameRepository');
const GuildNameRepository = require('./repositories/GuildNameRepository');
const InstagramCheckerRepository = require('./repositories/InstagramCheckerRepository');
const RedditCheckerRepository = require('./repositories/RedditCheckerRepository');
const YoutubeCheckerRepository = require('./repositories/YoutubeCheckerRepository');
const TumblrCheckerRepository = require('./repositories/TumblrCheckerRepository');
const GuildCommandRepository = require('./repositories/GuildCommandRepository');
const CommandRepository = require('./repositories/CommandRepository');
const UserGroupRepository = require('./repositories/UserGroupRepository');
const RoleGroupRepository = require('./repositories/RoleGroupRepository');
const SpecialRoleRepository = require('./repositories/SpecialRoleRepository');
const ReminderRepository = require('./repositories/ReminderRepository');
const TextChannelRepository = require('./repositories/TextChannelRepository');
const CleverBotSessionRepository = require('./repositories/CleverBotSessionRepository.js');

class DatabaseDriver {
    async load() {
        this._db = await massive(PostgreSQLConfig, {
            'scripts': `${__dirname}/scripts/`
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
        this.cleverbotSessions = new CleverBotSessionRepository(this._db);
    }
}

module.exports = DatabaseDriver;