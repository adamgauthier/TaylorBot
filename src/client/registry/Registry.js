'use strict';

const { Paths } = require('globalobjects');

const Log = require('../../tools/Logger.js');
const TypeRegistry = require(Paths.TypeRegistry);
const MessageWatcherRegistry = require('./MessageWatcherRegistry.js');
const GroupRegistry = require(Paths.GroupRegistry);
const GuildRegistry = require(Paths.GuildRegistry);
const GuildRoleGroupRegistry = require(Paths.GuildRoleGroupRegistry);
const CommandRegistry = require(Paths.CommandRegistry);
const UserRegistry = require(Paths.UserRegistry);
const InhibitorRegistry = require(Paths.InhibitorRegistry);

class Registry {
    constructor(database) {
        this.inhibitors = new InhibitorRegistry();
        this.types = new TypeRegistry();
        this.watchers = new MessageWatcherRegistry();
        this.groups = new GroupRegistry(database);
        this.guilds = new GuildRegistry(database);
        this.roleGroups = new GuildRoleGroupRegistry(database, this.guilds);
        this.commands = new CommandRegistry(database);
        this.users = new UserRegistry(database);
    }

    async load() {
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