'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class LocationAttributeRepository {
    constructor(db) {
        this._db = db;
    }

    async get(user) {
        try {
            return await this._db.oneOrNone(
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

    async set(user, formattedAddress, longitude, latitude, timezoneId) {
        try {
            return await this._db.one(
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

    async clear(user) {
        try {
            return await this._db.oneOrNone(
                `DELETE FROM attributes.location_attributes
                WHERE user_id = $[user_id]
                RETURNING *;`,
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

module.exports = LocationAttributeRepository;