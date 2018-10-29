'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class RoleGroupRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.instance.any('SELECT * FROM guilds.guild_role_groups;');
        }
        catch (e) {
            Log.error(`Getting all guild role groups: ${e}`);
            throw e;
        }
    }

    async add(role, group) {
        try {
            return await this._db.instance.one(
                'INSERT INTO guilds.guild_role_groups (guild_id, role_id, group_name) VALUES ($[guild_id], $[role_id], $[group_name]) RETURNING *;',
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