'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all('SELECT id, logOf FROM logChannel;', (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            rows.forEach(async channel => {
                await pg_db.guilds.text_channels.update(
                    {
                        'guild_id': channel.logOf,
                        'channel_id': channel.id
                    },
                    {
                        'is_logging': true
                    }
                );
            });
        }
    });
};

migrate();