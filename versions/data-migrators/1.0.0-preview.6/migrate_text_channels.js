'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    const registeredAt = Date.now();

    sqlite_db.all('SELECT id, serverId, messages FROM channel;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_channels = rows.map(channel => {
                return {
                    'channel_id': channel.id,
                    'guild_id': channel.serverId,
                    'registered_at': registeredAt,
                    'messages_count': channel.messages
                };
            });

            await pg_db.guilds.text_channels.insert(pg_channels);
        }
    });
};

migrate();