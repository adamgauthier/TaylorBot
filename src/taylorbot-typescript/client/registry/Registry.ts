import { Log } from '../../tools/Logger';
import { TypeRegistry } from './TypeRegistry';
import { MessageWatcherRegistry } from './MessageWatcherRegistry';
import { GroupRegistry } from './GroupRegistry';
import { GuildRegistry } from './GuildRegistry';
import { GuildRoleGroupRegistry } from './GuildRoleGroupRegistry';
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
    redis: RedisDriver;

    constructor(database: DatabaseDriver, redis: RedisDriver) {
        this.redis = redis;
        this.attributes = new AttributeRegistry(database);
        this.inhibitors = new InhibitorRegistry();
        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(database);
        this.guilds = new GuildRegistry(database, redis);
        this.roleGroups = new GuildRoleGroupRegistry(database, this.guilds);
        this.commands = new CommandRegistry(database, redis);
        this.users = new UserRegistry(database, redis);
        this.channelCommands = new ChannelCommandRegistry(database, redis);
        this.cooldowns = new CooldownRegistry(redis);
        this.onGoingCommands = new OnGoingCommandRegistry(redis);
        this.heists = new HeistRegistry(redis);
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
