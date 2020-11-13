import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';

export class UserGroupRepository {
    readonly #db: pgPromise.IDatabase<unknown>;
    readonly #helpers: pgPromise.IHelpers;
    readonly #columnSet: pgPromise.ColumnSet;

    constructor(db: pgPromise.IDatabase<unknown>, helpers: pgPromise.IHelpers) {
        this.#db = db;
        this.#helpers = helpers;
        this.#columnSet = new this.#helpers.ColumnSet(['name', 'access_level'], {
            table: new this.#helpers.TableName({ schema: 'commands', table: 'user_groups' })
        });
    }

    async getAll(): Promise<{ name: string; access_level: number }[]> {
        try {
            return await this.#db.any('SELECT * FROM commands.user_groups;');
        }
        catch (e) {
            Log.error(`Getting all user groups: ${e}`);
            throw e;
        }
    }

    async addAll(userGroups: { name: string; access_level: number }[]): Promise<void> {
        try {
            await this.#db.none(
                `${this.#helpers.insert(userGroups, this.#columnSet)};`
            );
        }
        catch (e) {
            Log.error(`Adding user groups: ${e}`);
            throw e;
        }
    }
}
