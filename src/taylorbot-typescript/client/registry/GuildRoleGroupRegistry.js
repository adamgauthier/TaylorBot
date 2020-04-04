'use strict';

const Format = require('../../modules/DiscordFormatter.js');

class GuildRoleGroupRegistry {
    constructor(database, guildRegistry) {
        this.database = database;
        this.guildRegistry = guildRegistry;
    }

    async load() {
        const guildRoleGroups = await this.database.roleGroups.getAll();
        guildRoleGroups.forEach(rg => this.cacheRoleGroup(rg));
    }

    cacheRoleGroup(databaseRoleGroup) {
        const guild = this.guildRegistry.get(databaseRoleGroup.guild_id);
        if (!guild)
            throw new Error(`Could not cache role group ${databaseRoleGroup.group_name} because the guild ${databaseRoleGroup.guild_id} was not cached.`);

        guild.roleGroups[databaseRoleGroup.role_id] = databaseRoleGroup.group_name;
    }

    async addRoleGroup(role, group) {
        const inserted = await this.database.roleGroups.add(role, group);
        this.cacheRoleGroup(inserted);
    }

    getRoleGroup(role, group) {
        const guild = this.guildRegistry.get(role.guild.id);
        if (!guild)
            throw new Error(`Could not verify role ${Format.role(role)} group '${group.name}' because the guild ${Format.guild(role.guild)} was not cached.`);

        return guild.roleGroups[role.id];
    }
}

module.exports = GuildRoleGroupRegistry;