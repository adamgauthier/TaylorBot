'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, oldminutes FROM user WHERE oldminutes > 0;`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_old_minutes = rows.map(oldMinute => ({
                'user_id': oldMinute.id,
                'attribute_id': 'oldminutes',
                'integer_value': oldMinute.oldminutes
            }));

            await pg_db.attributes.integer_attributes.insert(pg_old_minutes);
        }
    });
};

migrate();