'use strict';

const { Paths } = require('globalobjects');

const Log = require('../../tools/Logger.js');
const UserGroups = require(Paths.UserGroups);

class GroupRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async loadAll() {
        let userGroups = await this.database.userGroups.getAll();
        const defaults = Object.values(UserGroups).filter(d => !d.isSpecial);

        const defaultGroupsNotInDatabase = defaults.filter(d =>
            !userGroups.some(ug => ug.name === d.name)
        );

        if (defaultGroupsNotInDatabase.length > 0) {
            Log.info(`Found new default user groups ${defaultGroupsNotInDatabase.map(g => g.name).join(',')}. Adding to database.`);

            await this.database.userGroups.addAll(defaultGroupsNotInDatabase.map(d => {
                return { 'name': d.name, 'access_level': d.accessLevel };
            }));

            userGroups = await this.database.userGroups.getAll();
        }

        userGroups.forEach(ug => this.cacheGroup(ug.name, ug.access_level));
    }

    cacheGroup(name, accessLevel) {
        if (this.has(name))
            throw new Error(`Caching user group ${name}, was already cached.`);

        this.set(name, {
            name,
            accessLevel
        });
    }
}

module.exports = GroupRegistry;