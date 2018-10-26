'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class SpecialRoleRepository {
    constructor(db) {
        this._db = db;
    }

    mapRoleToDatabase(role) {
        return {
            'role_id': role.id,
            'guild_id': role.guild.id
        };
    }

    async get(role) {
        const databaseRole = this.mapRoleToDatabase(role);
        try {
            return await this._db.instance.oneOrNone(
                'SELECT * FROM guilds.guild_special_roles WHERE guild_id = $[guild_id] AND role_id = $[role_id];',
                databaseRole
            );
        }
        catch (e) {
            Log.error(`Getting special role ${Format.role(role)}: ${e}`);
            throw e;
        }
    }

    async _setAccessible(role, accessible) {
        const databaseRole = this.mapRoleToDatabase(role);
        const fields = { accessible };
        try {
            const inserted = await this._db.guilds.guild_special_roles.insert({ ...databaseRole, ...fields }, { 'onConflictIgnore': true });
            return inserted ? inserted : await this._db.guilds.guild_special_roles.update(databaseRole, fields, { 'single': true });
        }
        catch (e) {
            Log.error(`Setting accessible special role ${Format.role(role)} to ${accessible}: ${e}`);
            throw e;
        }
    }

    setAccessible(role) {
        return this._setAccessible(role, true);
    }

    removeAccessible(role) {
        return this._setAccessible(role, false);
    }
}

module.exports = SpecialRoleRepository;