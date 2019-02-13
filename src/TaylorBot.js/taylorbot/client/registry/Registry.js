'use strict';

const Log = require('../../tools/Logger.js');
const TypeRegistry = require('./TypeRegistry.js');
const MessageWatcherRegistry = require('./MessageWatcherRegistry.js');
const GroupRegistry = require('./GroupRegistry.js');
const GuildRegistry = require('./GuildRegistry.js');
const GuildRoleGroupRegistry = require('./GuildRoleGroupRegistry.js');
const CommandRegistry = require('./CommandRegistry.js');
const UserRegistry = require('./UserRegistry.js');
const InhibitorRegistry = require('./InhibitorRegistry.js');
const AttributeRegistry = require('./AttributeRegistry.js');
const ChannelCommandRegistry = require('./ChannelCommandRegistry.js');
const CooldownRegistry = require('./CooldownRegistry.js');
const AnsweredCooldownRegistry = require('./AnsweredCooldownRegistry.js');
const HeistRegistry = require('./HeistRegistry.js');

class Registry {
    constructor(database, redis, redisHeists) {
        this.attributes = new AttributeRegistry(database);
        this.inhibitors = new InhibitorRegistry();
        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(database);
        this.guilds = new GuildRegistry(database);
        this.roleGroups = new GuildRoleGroupRegistry(database, this.guilds);
        this.commands = new CommandRegistry(database);
        this.users = new UserRegistry(database);
        this.channelCommands = new ChannelCommandRegistry(database, redis);
        this.cooldowns = new CooldownRegistry(redis);
        this.answeredCooldowns = new AnsweredCooldownRegistry(redis);
        this.heists = new HeistRegistry(redisHeists);
    }

    async load() {
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

        Log.info('Loading users...');
        await this.users.load();
        Log.info('Users loaded!');

        Log.info('Caching channel commands...');
        await this.channelCommands.cacheChannelCommands();
        Log.info('Channel commands cached!');
    }
}

module.exports = Registry;