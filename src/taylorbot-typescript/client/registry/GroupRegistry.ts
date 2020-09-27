import { DatabaseDriver } from '../../database/DatabaseDriver';
import { Log } from '../../tools/Logger';
import UserGroups = require('../UserGroups');

export class GroupRegistry extends Map<string, { name: string; accessLevel: number }> {
    readonly #database: DatabaseDriver;

    constructor(database: DatabaseDriver) {
        super();
        this.#database = database;
    }

    async loadAll(): Promise<void> {
        let userGroups = await this.#database.userGroups.getAll();
        const defaults = Object.values(UserGroups).filter(d => !d.isSpecial);

        const defaultGroupsNotInDatabase = defaults.filter(d =>
            !userGroups.some(ug => ug.name === d.name)
        );

        if (defaultGroupsNotInDatabase.length > 0) {
            Log.info(`Found new default user groups ${defaultGroupsNotInDatabase.map(g => g.name).join(',')}. Adding to database.`);

            const mapped = defaultGroupsNotInDatabase.map(d => ({
                'name': d.name, 'access_level': d.accessLevel
            }));

            await this.#database.userGroups.addAll(mapped);

            userGroups = await this.#database.userGroups.getAll();
        }

        userGroups.forEach(ug => this.cacheGroup(ug.name, ug.access_level));
    }

    cacheGroup(name: string, accessLevel: number): void {
        if (this.has(name))
            throw new Error(`Caching user group ${name}, was already cached.`);

        this.set(name, {
            name,
            accessLevel
        });
    }
}
