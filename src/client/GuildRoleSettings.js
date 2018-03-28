'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class GuildRoleSettings extends Map {
    constructor(database, guildSettings) {
        super();
        this.database = database;
        this.guildSettings = guildSettings;
    }

    async load() {
        const guildRoleGroups = await this.database.getAllGuildRoleGroups();
        guildRoleGroups.forEach(rg => this.cacheRoleGroup(rg));
    }

    cacheRoleGroup(databaseRoleGroup) {
        const guild = this.guildSettings.get(databaseRoleGroup.guild_id);
        if (!guild)
            throw new Error(`Could not cache role group ${databaseRoleGroup.group_name} because the guild ${databaseRoleGroup.guild_id} was not cached.`);

        guild.roleGroups[databaseRoleGroup.role_id] = databaseRoleGroup.group_name;
        this.set(databaseRoleGroup.guild_id, guild);
    }
}

module.exports = GuildRoleSettings;