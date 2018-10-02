'use strict';

const massive = require('massive');

const PostgreSQLConfig = require('../config/postgresql.json');

const GuildRepository = require('./repositories/GuildRepository.js');
const UserRepository = require('./repositories/UserRepository.js');
const GuildMemberRepository = require('./repositories/GuildMemberRepository.js');
const UsernameRepository = require('./repositories/UsernameRepository.js');
const GuildNameRepository = require('./repositories/GuildNameRepository.js');
const InstagramCheckerRepository = require('./repositories/InstagramCheckerRepository.js');
const RedditCheckerRepository = require('./repositories/RedditCheckerRepository.js');
const YoutubeCheckerRepository = require('./repositories/YoutubeCheckerRepository.js');
const TumblrCheckerRepository = require('./repositories/TumblrCheckerRepository.js');
const GuildCommandRepository = require('./repositories/GuildCommandRepository.js');
const CommandRepository = require('./repositories/CommandRepository.js');
const UserGroupRepository = require('./repositories/UserGroupRepository.js');
const RoleGroupRepository = require('./repositories/RoleGroupRepository.js');
const SpecialRoleRepository = require('./repositories/SpecialRoleRepository.js');
const ReminderRepository = require('./repositories/ReminderRepository.js');
const TextChannelRepository = require('./repositories/TextChannelRepository.js');
const CleverBotSessionRepository = require('./repositories/CleverBotSessionRepository.js');
const AttributeRepository = require('./repositories/AttributeRepository.js');
const TextAttributeRepository = require('./repositories/TextAttributeRepository.js');
const IntegerAttributeRepository = require('./repositories/IntegerAttributeRepository.js');
const LocationAttributeRepository = require('./repositories/LocationAttributeRepository.js');

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
        this.attributes = new AttributeRepository(this._db);
        this.textAttributes = new TextAttributeRepository(this._db);
        this.integerAttributes = new IntegerAttributeRepository(this._db);
        this.locationAttributes = new LocationAttributeRepository(this._db);
    }
}

module.exports = DatabaseDriver;