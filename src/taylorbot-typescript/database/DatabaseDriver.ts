import * as pgPromise from 'pg-promise';

const pgp = pgPromise({
    capSQL: true
});

import PostgreSQLConfig = require('../config/postgresql.json');

import UserDAO = require('./dao/UserDAO.js');

import GuildRepository = require('./repositories/GuildRepository.js');
import UserRepository = require('./repositories/UserRepository.js');
import GuildMemberRepository = require('./repositories/GuildMemberRepository.js');
import { UsernameRepository } from './repositories/UsernameRepository';
import { GuildNameRepository } from './repositories/GuildNameRepository';
import GuildCommandRepository = require('./repositories/GuildCommandRepository.js');
import { CommandRepository } from './repositories/CommandRepository';
import UserGroupRepository = require('./repositories/UserGroupRepository.js');
import RoleGroupRepository = require('./repositories/RoleGroupRepository.js');
import ReminderRepository = require('./repositories/ReminderRepository.js');
import TextChannelRepository = require('./repositories/TextChannelRepository.js');
import { AttributeRepository } from './repositories/AttributeRepository';
import TextAttributeRepository = require('./repositories/TextAttributeRepository.js');
import IntegerAttributeRepository = require('./repositories/IntegerAttributeRepository.js');
import LocationAttributeRepository = require('./repositories/LocationAttributeRepository.js');
import RollStatsRepository = require('./repositories/RollStatsRepository.js');
import RpsStatsRepository = require('./repositories/RpsStatsRepository.js');
import GambleStatsRepository = require('./repositories/GambleStatsRepository.js');
import { ChannelCommandRepository } from './repositories/ChannelCommandRepository';
import HeistStatsRepository = require('./repositories/HeistStatsRepository.js');
import { BirthdayAttributeRepository } from './repositories/BirthdayAttributeRepository';
import { ProRepository } from './repositories/ProRepository';

export class DatabaseDriver {
    readonly guilds: GuildRepository;
    readonly users: UserRepository;
    readonly guildMembers: GuildMemberRepository;
    readonly usernames: UsernameRepository;
    readonly guildNames: GuildNameRepository;
    readonly guildCommands: GuildCommandRepository;
    readonly commands: CommandRepository;
    readonly userGroups: UserGroupRepository;
    readonly roleGroups: RoleGroupRepository;
    readonly reminders: ReminderRepository;
    readonly textChannels: TextChannelRepository;
    readonly attributes: AttributeRepository;
    readonly textAttributes: TextAttributeRepository;
    readonly integerAttributes: IntegerAttributeRepository;
    readonly locationAttributes: LocationAttributeRepository;
    readonly rollStats: RollStatsRepository;
    readonly rpsStats: RpsStatsRepository;
    readonly gambleStats: GambleStatsRepository;
    readonly channelCommands: ChannelCommandRepository;
    readonly heistStats: HeistStatsRepository;
    readonly birthdays: BirthdayAttributeRepository;
    readonly pros: ProRepository;

    constructor() {
        const db = pgp(PostgreSQLConfig);
        const helpers = pgp.helpers;

        const usersDAO = new UserDAO();

        this.guilds = new GuildRepository(db);
        this.users = new UserRepository(db, usersDAO);
        this.guildMembers = new GuildMemberRepository(db, helpers);
        this.usernames = new UsernameRepository(db);
        this.guildNames = new GuildNameRepository(db);
        this.guildCommands = new GuildCommandRepository(db);
        this.commands = new CommandRepository(db);
        this.userGroups = new UserGroupRepository(db, helpers);
        this.roleGroups = new RoleGroupRepository(db);
        this.reminders = new ReminderRepository(db);
        this.textChannels = new TextChannelRepository(db);
        this.attributes = new AttributeRepository(db, helpers);
        this.textAttributes = new TextAttributeRepository(db);
        this.integerAttributes = new IntegerAttributeRepository(db);
        this.locationAttributes = new LocationAttributeRepository(db);
        this.rollStats = new RollStatsRepository(db, usersDAO);
        this.rpsStats = new RpsStatsRepository(db, usersDAO);
        this.gambleStats = new GambleStatsRepository(db, usersDAO);
        this.channelCommands = new ChannelCommandRepository(db);
        this.heistStats = new HeistStatsRepository(db, usersDAO);
        this.birthdays = new BirthdayAttributeRepository(db);
        this.pros = new ProRepository(db);
    }
}
