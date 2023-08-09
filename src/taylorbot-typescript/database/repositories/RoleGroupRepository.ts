import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';

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
}
