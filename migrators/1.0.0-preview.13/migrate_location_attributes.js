'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');
const GooglePlacesModule = require('../../src/modules/google/GooglePlacesModule.js');
const GoogleTimezoneModule = require('../../src/modules/google/GoogleTimezoneModule.js');

process.on('unhandledRejection', reason => { throw reason; });

const wait = msToWait => {
    return new Promise(resolve => {
        setTimeout(resolve, msToWait);
    });
}

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, location FROM user WHERE location IS NOT NULL AND location != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            for (const row of rows) {
                console.log(`${row.id}: ${row.location}`);

                const placeResponse = await GooglePlacesModule.findPlaceFromText(row.location);
                if (placeResponse.status !== 'OK') {
                    console.log(`PLACE: ${placeResponse.status}`);
                    break;
                }

                const { geometry: { location: { lat, lng } }, formatted_address } = placeResponse.candidates[0];
                const response = await GoogleTimezoneModule.getCurrentTimeForLocation(lat, lng);

                if (response.status !== 'OK') {
                    console.log(`TIME: ${response.status}`);
                    break;
                }

                console.log(`${row.id}: ${row.location} -> ${formatted_address} / ${response.timeZoneId}`);

                await pg_db.attributes.location_attributes.insert({
                    'user_id': row.id,
                    formatted_address,
                    'longitude': lng,
                    'latitude': lat,
                    'timezone_id': response.timeZoneId
                });

                await wait(1000);
            }
        }
    });
};

migrate();