import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildRegistry } from './GuildRegistry';

export class GuildRoleGroupRegistry {
    readonly #database: DatabaseDriver;
    readonly #guildRegistry: GuildRegistry;

    constructor(database: DatabaseDriver, guildRegistry: GuildRegistry) {
        this.#database = database;
        this.#guildRegistry = guildRegistry;
    }

    async load(): Promise<void> {
        const guildRoleGroups = await this.#database.roleGroups.getAll();
        guildRoleGroups.forEach(rg => this.cacheRoleGroup(rg));
    }

    cacheRoleGroup(databaseRoleGroup: { guild_id: string; role_id: string; group_name: string }): void {
        const guild = this.#guildRegistry.get(databaseRoleGroup.guild_id);
        if (!guild)
            throw new Error(`Could not cache role group ${databaseRoleGroup.group_name} because the guild ${databaseRoleGroup.guild_id} was not cached.`);

        guild.roleGroups[databaseRoleGroup.role_id] = databaseRoleGroup.group_name;
    }
}
