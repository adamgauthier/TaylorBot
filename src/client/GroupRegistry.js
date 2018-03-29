'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const DefaultGroups = require(GlobalPaths.DefaultGroups);

class GroupRegistry extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async loadAll() {
        let userGroups = await this.database.getAllUserGroups();
        const defaults = Object.values(DefaultGroups).filter(d => !d.isSpecial);

        const defaultGroupsNotInDatabase = defaults.filter(d =>
            !userGroups.some(ug => ug.name === d.name)
        );

        if (defaultGroupsNotInDatabase.length > 0) {
            Log.info(`Found new default user groups ${defaultGroupsNotInDatabase.map(g => g.name).join(',')}. Adding to database.`);

            await this.database.addUserGroups(defaultGroupsNotInDatabase.map(d => {
                return { 'name': d.name, 'access_level': d.accessLevel };
            }));

            userGroups = await this.database.getAllUserGroups();
        }

        userGroups.forEach(ug => this.cacheGroup(ug.name, ug.access_level));
    }

    cacheGroup(name, accessLevel) {
        if (this.has(name))
            Log.warn(`Caching user group ${name}, was already cached, overwriting.`);

        this.set(name, accessLevel);
    }
}

module.exports = GroupRegistry;