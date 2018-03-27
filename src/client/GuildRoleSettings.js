'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);

class GuildRoleSettings extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async load() {
        const guildRoleGroups = await this.database.getAllGuildRoleGroups();
        guildRoleGroups.forEach(rg => this.cacheRoleGroup(rg));
    }

    cacheRoleGroup(databaseRoleGroup) {
        let guildRole = this.get(databaseRoleGroup.guild_id);

        if (!guildRole)
            guildRole = {};

        guildRole[databaseRoleGroup.role_id] = databaseRoleGroup.group_name;
        this.set(databaseRoleGroup.guild_id, guildRole);
    }
}

module.exports = GuildRoleSettings;