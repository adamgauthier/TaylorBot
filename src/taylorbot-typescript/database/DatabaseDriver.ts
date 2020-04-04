import * as pgPromise from 'pg-promise';

const pgp = pgPromise({
    capSQL: true
});

import PostgreSQLConfig = require('../config/postgresql.json');

import UserDAO = require('./dao/UserDAO.js');

import GuildRepository = require('./repositories/GuildRepository.js');
import UserRepository = require('./repositories/UserRepository.js');
import GuildMemberRepository = require('./repositories/GuildMemberRepository.js');
import UsernameRepository = require('./repositories/UsernameRepository.js');
import GuildNameRepository = require('./repositories/GuildNameRepository.js');
import InstagramCheckerRepository = require('./repositories/InstagramCheckerRepository.js');
import GuildCommandRepository = require('./repositories/GuildCommandRepository.js');
import CommandRepository = require('./repositories/CommandRepository.js');
import UserGroupRepository = require('./repositories/UserGroupRepository.js');
import RoleGroupRepository = require('./repositories/RoleGroupRepository.js');
import SpecialRoleRepository = require('./repositories/SpecialRoleRepository.js');
import ReminderRepository = require('./repositories/ReminderRepository.js');
import TextChannelRepository = require('./repositories/TextChannelRepository.js');
import AttributeRepository = require('./repositories/AttributeRepository.js');
import TextAttributeRepository = require('./repositories/TextAttributeRepository.js');
import IntegerAttributeRepository = require('./repositories/IntegerAttributeRepository.js');
import LocationAttributeRepository = require('./repositories/LocationAttributeRepository.js');
import RollStatsRepository = require('./repositories/RollStatsRepository.js');
import RpsStatsRepository = require('./repositories/RpsStatsRepository.js');
import GambleStatsRepository = require('./repositories/GambleStatsRepository.js');
import DailyPayoutRepository = require('./repositories/DailyPayoutRepository.js');
import ChannelCommandRepository = require('./repositories/ChannelCommandRepository.js');
import HeistStatsRepository = require('./repositories/HeistStatsRepository.js');
import BirthdayAttributeRepository = require('./repositories/BirthdayAttributeRepository.js');
import ProRepository = require('./repositories/ProRepository.js');

export class DatabaseDriver {
    guilds: GuildRepository;
    users: UserRepository;
    guildMembers: GuildMemberRepository;
    usernames: UsernameRepository;
    guildNames: GuildNameRepository;
    instagramCheckers: InstagramCheckerRepository;
    guildCommands: GuildCommandRepository;
    commands: CommandRepository;
    userGroups: UserGroupRepository;
    roleGroups: RoleGroupRepository;
    specialRoles: SpecialRoleRepository;
    reminders: ReminderRepository;
    textChannels: TextChannelRepository;
    attributes: AttributeRepository;
    textAttributes: TextAttributeRepository;
    integerAttributes: IntegerAttributeRepository;
    locationAttributes: LocationAttributeRepository;
    rollStats: RollStatsRepository;
    rpsStats: RpsStatsRepository;
    gambleStats: GambleStatsRepository;
    dailyPayouts: DailyPayoutRepository;
    channelCommands: ChannelCommandRepository;
    heistStats: HeistStatsRepository;
    birthdays: BirthdayAttributeRepository;
    pros: ProRepository;

    constructor() {
        const db = pgp(PostgreSQLConfig);
        const helpers = pgp.helpers;

        const usersDAO = new UserDAO();

        this.guilds = new GuildRepository(db);
        this.users = new UserRepository(db, usersDAO);
        this.guildMembers = new GuildMemberRepository(db, helpers);
        this.usernames = new UsernameRepository(db);
        this.guildNames = new GuildNameRepository(db);
        this.instagramCheckers = new InstagramCheckerRepository(db);
        this.guildCommands = new GuildCommandRepository(db);
        this.commands = new CommandRepository(db, helpers);
        this.userGroups = new UserGroupRepository(db, helpers);
        this.roleGroups = new RoleGroupRepository(db);
        this.specialRoles = new SpecialRoleRepository(db);
        this.reminders = new ReminderRepository(db);
        this.textChannels = new TextChannelRepository(db);
        this.attributes = new AttributeRepository(db, helpers);
        this.textAttributes = new TextAttributeRepository(db);
        this.integerAttributes = new IntegerAttributeRepository(db);
        this.locationAttributes = new LocationAttributeRepository(db);
        this.rollStats = new RollStatsRepository(db, usersDAO);
        this.rpsStats = new RpsStatsRepository(db, usersDAO);
        this.gambleStats = new GambleStatsRepository(db, usersDAO);
        this.dailyPayouts = new DailyPayoutRepository(db, usersDAO);
        this.channelCommands = new ChannelCommandRepository(db);
        this.heistStats = new HeistStatsRepository(db, usersDAO);
        this.birthdays = new BirthdayAttributeRepository(db);
        this.pros = new ProRepository(db);
    }
}
