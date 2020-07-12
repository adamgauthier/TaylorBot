import Log = require('../../tools/Logger.js');
import TypeRegistry = require('./TypeRegistry.js');
import { MessageWatcherRegistry } from './MessageWatcherRegistry';
import GroupRegistry = require('./GroupRegistry.js');
import { GuildRegistry } from './GuildRegistry';
import GuildRoleGroupRegistry = require('./GuildRoleGroupRegistry.js');
import { CommandRegistry } from './CommandRegistry';
import { UserRegistry } from './UserRegistry';
import { InhibitorRegistry } from './InhibitorRegistry';
import { AttributeRegistry } from './AttributeRegistry';
import { ChannelCommandRegistry } from './ChannelCommandRegistry';
import { CooldownRegistry } from './CooldownRegistry';
import { OnGoingCommandRegistry } from './OnGoingCommandRegistry';
import { HeistRegistry } from './HeistRegistry';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { RedisDriver } from '../../caching/RedisDriver';

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
    redisCommands: RedisDriver;

    constructor(database: DatabaseDriver, redisCommands: RedisDriver, redisHeists: RedisDriver) {
        this.redisCommands = redisCommands;
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
