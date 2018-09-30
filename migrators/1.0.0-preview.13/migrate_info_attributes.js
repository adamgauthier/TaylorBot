'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, age FROM user WHERE age IS NOT NULL;`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_ages = rows.map(age => {
                return {
                    'user_id': age.id,
                    'attribute_id': 'age',
                    'attribute_value': age.age
                };
            });

            await pg_db.attributes.integer_attributes.insert(pg_ages);
        }
    });
};

migrate();