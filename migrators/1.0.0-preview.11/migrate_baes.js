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
};

migrate();