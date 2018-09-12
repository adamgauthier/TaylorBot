'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, instagram FROM user WHERE instagram IS NOT NULL AND instagram != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_instagrams = rows.map(instagram => {
                return {
                    'user_id': instagram.id,
                    'attribute_id': 'instagram',
                    'attribute_value': instagram.instagram
                };
            });

            await pg_db.attributes.text_attributes.insert(pg_instagrams);
        }
    });

    sqlite_db.all(`SELECT id, snapchat FROM user WHERE snapchat IS NOT NULL AND snapchat != '';`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_snapchats = rows.map(snapchat => {
                return {
                    'user_id': snapchat.id,
                    'attribute_id': 'snapchat',
                    'attribute_value': snapchat.snapchat
                };
            });

            await pg_db.attributes.text_attributes.insert(pg_snapchats);
        }
    });
};

migrate();