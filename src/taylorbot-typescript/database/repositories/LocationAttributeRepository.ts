import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { User } from 'discord.js';

export class LocationAttributeRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    async get(user: User): Promise<{ user_id: string; formatted_address: string; longitude: string; latitude: string; timezone_id: string } | null> {
        try {
            return await this.#db.oneOrNone(
                `SELECT * FROM attributes.location_attributes
                WHERE user_id = $[user_id];`,
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting location for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async set(user: User, formattedAddress: string, longitude: string, latitude: string, timezoneId: string): Promise<{ user_id: string; formatted_address: string; longitude: string; latitude: string; timezone_id: string }> {
        try {
            return await this.#db.one(
                `INSERT INTO attributes.location_attributes (user_id, formatted_address, longitude, latitude, timezone_id)
                VALUES ($[user_id], $[formatted_address], $[longitude], $[latitude], $[timezone_id])
                ON CONFLICT (user_id) DO UPDATE SET
                  formatted_address = excluded.formatted_address,
                  longitude = excluded.longitude,
                  latitude = excluded.latitude,
                  timezone_id = excluded.timezone_id
                RETURNING *;`,
                {
                    'user_id': user.id,
                    'formatted_address': formattedAddress,
                    'longitude': longitude,
                    'latitude': latitude,
                    'timezone_id': timezoneId
                }
            );
        }
        catch (e) {
            Log.error(`Setting location to '${formattedAddress}' for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async clear(user: User): Promise<void> {
        try {
            await this.#db.none(
                `DELETE FROM attributes.location_attributes
                WHERE user_id = $[user_id];`,
                {
                    'user_id': user.id
                }
            );
        }
        catch (e) {
            Log.error(`Clearing location for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}
