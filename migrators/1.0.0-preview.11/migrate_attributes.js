'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, bae FROM user WHERE bae IS NOT NULL AND bae != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_baes = rows.map(bae => {
                return {
                    'user_id': bae.id,
                    'attribute_id': 'bae',
                    'attribute_value': bae.bae
                };
            });

            await pg_db.attributes.text_attributes.insert(pg_baes);
        }
    });

    sqlite_db.all(`SELECT id, favsong FROM user WHERE favsong IS NOT NULL AND favsong != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_favs = rows.map(fav => {
                return {
                    'user_id': fav.id,
                    'attribute_id': 'favoritesongs',
                    'attribute_value': fav.favsong
                };
            });

            await pg_db.attributes.text_attributes.insert(pg_favs);
        }
    });

    sqlite_db.all(`SELECT lastfm FROM user WHERE lastfm IS NOT NULL AND lastfm != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const regex = /^<http:\/\/www.last.fm\/user\/([a-z][a-z0-9_-]{1,14})>$/i;
            rows = rows.filter(r => regex.test(r.lastfm));

            const pg_fms = rows.map(fm => {
                const matches = regex.match(fm.lastfm);
                return {
                    'user_id': fm.id,
                    'attribute_id': 'lastfm',
                    'attribute_value': matches[1]
                };
            });

            await pg_db.attributes.text_attributes.insert(pg_fms);
        }
    });
};

migrate();