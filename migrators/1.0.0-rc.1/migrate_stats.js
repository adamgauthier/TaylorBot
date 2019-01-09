'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, MAX(rpswins) AS rpswins FROM userByServer WHERE rpswins > 0 GROUP BY id;`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            await pg_db.users.rps_stats.insert(rows.map(row => ({
                user_id: row.id,
                rps_wins: row.rpswins
            })));
        }
    });
};

migrate();