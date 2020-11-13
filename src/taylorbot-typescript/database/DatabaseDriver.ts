import * as pgPromise from 'pg-promise';

const pgp = pgPromise({
    capSQL: true
});

import PostgreSQLConfig = require('../config/postgresql.json');

import { UserDAO } from './dao/UserDAO';

import { GuildRepository } from './repositories/GuildRepository';
import { UserRepository } from './repositories/UserRepository';
import { GuildMemberRepository } from './repositories/GuildMemberRepository';
import { UsernameRepository } from './repositories/UsernameRepository';
import { GuildNameRepository } from './repositories/GuildNameRepository';
import { GuildCommandRepository } from './repositories/GuildCommandRepository';
import { CommandRepository } from './repositories/CommandRepository';
import { UserGroupRepository } from './repositories/UserGroupRepository';
import { RoleGroupRepository } from './repositories/RoleGroupRepository';
import { ReminderRepository } from './repositories/ReminderRepository';
import { TextChannelRepository } from './repositories/TextChannelRepository';
import { AttributeRepository } from './repositories/AttributeRepository';
import { TextAttributeRepository } from './repositories/TextAttributeRepository';
import { IntegerAttributeRepository } from './repositories/IntegerAttributeRepository';
import { LocationAttributeRepository } from './repositories/LocationAttributeRepository';
import { RollStatsRepository } from './repositories/RollStatsRepository';
import { RpsStatsRepository } from './repositories/RpsStatsRepository';
import { GambleStatsRepository } from './repositories/GambleStatsRepository';
import { ChannelCommandRepository } from './repositories/ChannelCommandRepository';
import { HeistStatsRepository } from './repositories/HeistStatsRepository';
import { BirthdayAttributeRepository } from './repositories/BirthdayAttributeRepository';
import { ProRepository } from './repositories/ProRepository';
import { EnvUtil } from '../modules/util/EnvUtil';

const postgresUsername = EnvUtil.getRequiredEnvVariable('TaylorBot_DatabaseConnection__Username');
const postgresPassword = EnvUtil.getRequiredEnvVariable('TaylorBot_DatabaseConnection__Password');

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
        const db = pgp({
            host: PostgreSQLConfig.host,
            port: PostgreSQLConfig.port,
            database: PostgreSQLConfig.database,
            user: postgresUsername,
            password: postgresPassword
        });
        const helpers = pgp.helpers;

        const usersDAO = new UserDAO();

        this.guilds = new GuildRepository(db);
        this.users = new UserRepository(db);
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
