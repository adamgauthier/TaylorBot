'use strict';

const { loginToken } = require('../../src/config/discord.json');
const PostgreSQLConfig = require('../postgresql.json');

const Discord = require('discord.js');
const massive = require('massive');
const fs = require('fs');
const sqlite3 = require('sqlite3').verbose();

const client = new Discord.Client();

const sqlite_db = new sqlite3.Database('old_database.db');
const getOldLastSpoke = (guild_member) => {
    return new Promise((resolve, reject) => {
        sqlite_db.get('SELECT lastSpoke FROM userByServer WHERE id = ? AND serverId = ?;', [guild_member.user_id, guild_member.guild_id], async (err, row) => {
            if (err)
                reject(err);
            else
                resolve(row);
        });
    });
};

client.on('ready', async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const noJoinedDates = await pg_db.guild_members.find(
        {
            'first_joined_at': 0
        },
        {
            'fields': ['guild_id', 'user_id']
        }
    );

    const updated = [];

    for (const guild of client.guilds.values()) {
        const guild_members = noJoinedDates.filter(gm => gm.guild_id === guild.id);
        if (guild_members.length > 0) {
            const members = await guild.members.fetch();

            for (const guild_member of guild_members) {
                const member = members.get(guild_member.user_id);
                if (member) {
                    await pg_db.guild_members.update(guild_member, {
                        'first_joined_at': member.joinedTimestamp
                    });
                    updated.push({
                        'guild_id': guild_member.guild_id,
                        'user_id': guild_member.user_id,
                        'updated_first_joined_at': member.joinedTimestamp
                    });
                }
                else {
                    const row = await getOldLastSpoke(guild_member);
                    if (!row)
                        throw row;

                    if (typeof row.lastSpoke !== 'number')
                        throw row.lastSpoke;

                    if (row.lastSpoke > 0) {
                        await pg_db.guild_members.update(guild_member, {
                            'first_joined_at': row.lastSpoke
                        });
                        updated.push({
                            'guild_id': guild_member.guild_id,
                            'user_id': guild_member.user_id,
                            'updated_first_joined_at': row.lastSpoke
                        });
                    }
                }
            }
        }
    }

    fs.writeFileSync('./updated_join_dates.json', JSON.stringify(updated, null, 2));

    process.exit();
});

client.login(loginToken);