'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const TypeRegistry = require(GlobalPaths.TypeRegistry);
const MessageWatcherRegistry = require(GlobalPaths.MessageWatcherRegistry);
const GroupRegistry = require(GlobalPaths.GroupRegistry);
const GuildRegistry = require(GlobalPaths.GuildRegistry);
const GuildRoleGroupRegistry = require(GlobalPaths.GuildRoleGroupRegistry);
const CommandRegistry = require(GlobalPaths.CommandRegistry);
const UserRegistry = require(GlobalPaths.UserRegistry);
const InhibitorRegistry = require(GlobalPaths.InhibitorRegistry);

class Registry {
    constructor(client) {
        this.client = client;

        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(this.client.master.database);
        this.guilds = new GuildRegistry(this.client);
        this.roleGroups = new GuildRoleGroupRegistry(this.client.master.database, this.guilds);
        this.commands = new CommandRegistry(this.client);
        this.users = new UserRegistry(this.client.master.database);
        this.inhibitors = new InhibitorRegistry();
    }

    async loadAll() {
        Log.info('Loading inhibitors...');
        await this.inhibitors.loadAll();
        Log.info('Inhibitors loaded!');
        
        Log.info('Loading types...');
        await this.types.loadAll(this.client);
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
    }
}

module.exports = Registry;