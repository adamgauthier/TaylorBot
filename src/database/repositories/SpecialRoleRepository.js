'use strict';

const { Paths } = require('globalobjects');

const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

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
            return await this._db.guild_special_roles.findOne(databaseRole);
        }
        catch (e) {
            Log.error(`Getting special role ${Format.role(role)}: ${e}`);
            throw e;
        }
    }

    async setAccessible(role) {
        const databaseRole = this.mapRoleToDatabase(role);
        const fields = { 'accessible': true };
        try {
            const inserted = await this._db.guild_special_roles.insert({ ...databaseRole, ...fields }, { 'onConflictIgnore': true });
            return inserted ? inserted : await this._db.guild_special_roles.update(databaseRole, fields, { 'single': true });
        }
        catch (e) {
            Log.error(`Setting accessible special role ${Format.role(role)}: ${e}`);
            throw e;
        }
    }
}

module.exports = SpecialRoleRepository;