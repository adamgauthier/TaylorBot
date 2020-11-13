import { Log } from '../../tools/Logger';
import * as pgPromise from 'pg-promise';

export class AttributeRepository {
    _db: pgPromise.IDatabase<unknown>;
    _helpers: pgPromise.IHelpers;
    _columnSet: pgPromise.ColumnSet;

    constructor(db: pgPromise.IDatabase<unknown>, helpers: pgPromise.IHelpers) {
        this._db = db;
        this._helpers = helpers;
        this._columnSet = new this._helpers.ColumnSet(['attribute_id'], {
            table: new this._helpers.TableName({ schema: 'attributes', table: 'attributes' })
        });
    }

    async getAll(): Promise<{ attribute_id: string }[]> {
        try {
            return await this._db.any('SELECT attribute_id FROM attributes.attributes;');
        }
        catch (e) {
            Log.error(`Getting all attributes: ${e}`);
            throw e;
        }
    }

    async addAll(databaseAttributes: { attribute_id: string }[]): Promise<void> {
        try {
            await this._db.any(
                `${this._helpers.insert(databaseAttributes, this._columnSet)};`,
            );
        }
        catch (e) {
            Log.error(`Adding attributes: ${e}`);
            throw e;
        }
    }
}
