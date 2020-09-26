import Log = require('../../tools/Logger.js');
import Format = require('../../modules/DiscordFormatter.js');
import * as pgPromise from 'pg-promise';
import { Role } from 'discord.js';

export class RoleGroupRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async getAll(): Promise<{ guild_id: string; role_id: string; group_name: string }[]> {
        try {
            return await this.#db.any('SELECT * FROM guilds.guild_role_groups;');
        }
        catch (e) {
            Log.error(`Getting all guild role groups: ${e}`);
            throw e;
        }
    }

    async add(role: Role, group: { name: string; accessLevel: number }): Promise<{ guild_id: string; role_id: string; group_name: string }> {
        try {
            return await this.#db.one(
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
