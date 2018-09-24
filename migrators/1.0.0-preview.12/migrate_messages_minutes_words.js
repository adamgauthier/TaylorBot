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
                    criteria: {
                        'guild_id': member.serverId,
                        'user_id': member.id,
                    },
                    update: {
                        'minute_count': member.minutes,
                        'message_count': member.messages,
                        'word_count': member.wordscount
                    }
                };
            });

            for (const member of pg_members) {
                await pg_db.guilds.guild_members.update(member.criteria, member.update);
            }
        }
    });
};

migrate();