'use strict';

const path = require('path');
const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

process.on('unhandledRejection', reason => { throw reason; });

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database(path.join(__dirname, 'old_database.db'));

    sqlite_db.all(`SELECT id, serverId, MAX(0, messages) AS messages, minutes, MAX(0, wordscount) AS wordscount FROM userByServer;`, async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_members = rows.map(member => {
                return {
                    'guild_id': member.serverId,
                    'user_id': member.id,
                    'minute_count': member.minutes,
                    'message_count': member.messages,
                    'word_count': member.wordscount,
                };
            });

            const halfMark = Math.floor(pg_members.length/2);
            const firstHalf = pg_members.slice(0, halfMark);
            const secondHalf = pg_members.slice(halfMark, pg_members.length);

            await pg_db.guilds.guild_members.insert(firstHalf);
            await pg_db.guilds.guild_members.insert(secondHalf);
        }
    });
};

migrate();