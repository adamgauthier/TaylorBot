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
    constructor(database) {
        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(database);
        this.guilds = new GuildRegistry(database);
        this.roleGroups = new GuildRoleGroupRegistry(database, this.guilds);
        this.commands = new CommandRegistry(database);
        this.users = new UserRegistry(database);
        this.inhibitors = new InhibitorRegistry();
    }

    async loadAll() {
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
    }
}

module.exports = Registry;