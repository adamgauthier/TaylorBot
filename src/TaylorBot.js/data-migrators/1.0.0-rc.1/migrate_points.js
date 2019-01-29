'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');
const excluded_members = require('./excluded_members.json');
const excluded_servers = require('./excluded_servers.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT u.id, serverId, taypoints FROM (
        SELECT id, MAX(taypoints) AS max_points
        FROM userByServer
        WHERE
        (serverId != $1 AND id != $2) AND
        (serverId != $3 AND id != $4) AND
        (serverId != $5 AND id != $6) AND
        serverId NOT IN ($7, $8, $9)
        GROUP BY id
    ) AS maxed INNER JOIN userByServer AS u ON u.id = maxed.id AND u.taypoints = maxed.max_points
    ORDER BY max_points DESC;`, [
            excluded_members[0].guild_id, excluded_members[0].user_id,
            excluded_members[1].guild_id, excluded_members[1].user_id,
            excluded_members[2].guild_id, excluded_members[2].user_id,
            excluded_servers[0], excluded_servers[1], excluded_servers[2]
        ], async (err, rows) => {
            if (err) {
                throw err;
            }
            else {
                for (const row of rows) {
                    await pg_db.users.users.update({ user_id: row.id }, { taypoint_count: row.taypoints });
                }
            }
        });
};

migrate();