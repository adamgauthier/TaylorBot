'use strict';

const { GlobalPaths } = require('globalobjects');

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class RoleGroupRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guild_role_groups.find();
        }
        catch (e) {
            Log.error(`Getting all guild role groups: ${e}`);
            throw e;
        }
    }

    async add(role, group) {
        try {
            return await this._db.guild_role_groups.insert(
                {
                    'guild_id': role.guild.id,
                    'role_id': role.id,
                    'group_name': group.name
                }
            );
        }
        catch (e) {
            Log.error(`Setting guild '${Format.guild(role.guild)}' role '${Format.role(role)}' group '${group.name}': ${e}`);
            throw e;
        }
    }
}

module.exports = RoleGroupRepository;