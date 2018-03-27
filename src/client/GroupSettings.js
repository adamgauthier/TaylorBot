'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const DefaultGroups = require(GlobalPaths.DefaultGroups);

class GroupSettings extends Map {
    constructor(database) {
        super();
        this.database = database;
    }

    async loadAll() {
        let userGroups = await this.database.getAllUserGroups();
        const defaults = Object.keys(DefaultGroups);

        const defaultGroupsNotInDatabase = defaults.filter(d =>
            !userGroups.some(ug => ug.name === d)
        );

        if (defaultGroupsNotInDatabase.length > 0) {
            Log.info(`Found new default user groups ${defaultGroupsNotInDatabase.join(',')}. Adding to database.`);

            await this.database.addUserGroups(defaultGroupsNotInDatabase.map(d => {
                return { 'name': d, 'access_level': DefaultGroups[d] };
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

module.exports = GroupSettings;