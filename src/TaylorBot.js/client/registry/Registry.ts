import Log = require('../../tools/Logger.js');
import TypeRegistry = require('./TypeRegistry.js');
import { MessageWatcherRegistry } from './MessageWatcherRegistry';
import GroupRegistry = require('./GroupRegistry.js');
import GuildRegistry = require('./GuildRegistry.js');
import GuildRoleGroupRegistry = require('./GuildRoleGroupRegistry.js');
import { CommandRegistry } from './CommandRegistry';
import UserRegistry = require('./UserRegistry.js');
import InhibitorRegistry = require('./InhibitorRegistry.js');
import AttributeRegistry = require('./AttributeRegistry.js');
import ChannelCommandRegistry = require('./ChannelCommandRegistry.js');
import CooldownRegistry = require('./CooldownRegistry.js');
import OnGoingCommandRegistry = require('./OnGoingCommandRegistry.js');
import HeistRegistry = require('./HeistRegistry.js');
import DatabaseDriver = require('../../database/DatabaseDriver.js');
import RedisDriver = require('../../caching/RedisDriver.js');

export class Registry {
    types: TypeRegistry;
    attributes: AttributeRegistry;
    inhibitors: InhibitorRegistry;
    watchers: MessageWatcherRegistry;
    groups: GroupRegistry;
    guilds: GuildRegistry;
    roleGroups: GuildRoleGroupRegistry;
    commands: CommandRegistry;
    users: UserRegistry;
    channelCommands: ChannelCommandRegistry;
    cooldowns: CooldownRegistry;
    onGoingCommands: OnGoingCommandRegistry;
    heists: HeistRegistry;

    constructor(database: DatabaseDriver, redisCommands: RedisDriver, redisHeists: RedisDriver) {
        this.attributes = new AttributeRegistry(database);
        this.inhibitors = new InhibitorRegistry();
        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(database);
        this.guilds = new GuildRegistry(database, redisCommands);
        this.roleGroups = new GuildRoleGroupRegistry(database, this.guilds);
        this.commands = new CommandRegistry(database, redisCommands);
        this.users = new UserRegistry(database, redisCommands);
        this.channelCommands = new ChannelCommandRegistry(database, redisCommands);
        this.cooldowns = new CooldownRegistry(redisCommands);
        this.onGoingCommands = new OnGoingCommandRegistry(redisCommands);
        this.heists = new HeistRegistry(redisHeists);
    }

    async load(): Promise<void> {
        Log.info('Loading attributes...');
        await this.attributes.loadAll();
        Log.info('Attributes loaded!');

        Log.info('Loading inhibitors...');
        await this.inhibitors.loadAll();
        Log.info('Inhibitors loaded!');

        Log.info('Loading types...');
        await this.types.loadAll();
        Log.info('Types loaded!');

        Log.info('Loading message watchers...');
        await this.watchers.loadAll();
        Log.info('Message watchers loaded!');

        Log.info('Loading groups...');
        await this.groups.loadAll();
        Log.info('Groups loaded!');

        Log.info('Loading guilds...');
        await this.guilds.load();
        Log.info('Guilds loaded!');

        Log.info('Loading role groups...');
        await this.roleGroups.load();
        Log.info('Role groups loaded!');

        Log.info('Loading commands...');
        await this.commands.loadAll();
        Log.info('Commands loaded!');
    }
}
