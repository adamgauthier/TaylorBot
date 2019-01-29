'use strict';

const massive = require('massive');
const sqlite3 = require('sqlite3').verbose();
const PostgreSQLConfig = require('../postgresql.json');

function wait(msToWait) {
    return new Promise(resolve => {
        setTimeout(resolve, msToWait);
    });
}

const migrate = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database('old_database.db');

    sqlite_db.all('SELECT id FROM user WHERE id IS NOT NULL AND id != \'1\';', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_users = rows.map(user => {
                return { 'user_id': user.id };
            });

            await pg_db.users.insert(pg_users);
        }
    });

    sqlite_db.all('SELECT id, prefix FROM server;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_guilds = rows.map(server => {
                return {
                    'guild_id': server.id,
                    'prefix': server.prefix
                };
            });

            await pg_db.guilds.insert(pg_guilds);
        }
    });

    await wait(10000);

    sqlite_db.all('SELECT id, serverId, firstJoinedAt FROM userByServer;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_guildmembers = rows.map(member => {
                return {
                    'user_id': member.id,
                    'guild_id': member.serverId,
                    'first_joined_at': member.firstJoinedAt
                };
            });

            const halfMark = Math.floor(pg_guildmembers.length/2);
            const firstHalf = pg_guildmembers.slice(0, halfMark);
            const secondHalf = pg_guildmembers.slice(halfMark, pg_guildmembers.length);

            await pg_db.guild_members.insert(firstHalf);
            await pg_db.guild_members.insert(secondHalf);
        }
    });

    sqlite_db.all('SELECT userId, username, since FROM usernames WHERE userId != \'1\';', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_usernames = rows.map(u => {
                return {
                    'user_id': u.userId,
                    'username': u.username,
                    'changed_at': u.since
                };
            });

            const halfMark = Math.floor(pg_usernames.length/2);
            const firstHalf = pg_usernames.slice(0, halfMark);
            const secondHalf = pg_usernames.slice(halfMark, pg_usernames.length);

            await pg_db.usernames.insert(firstHalf);
            await pg_db.usernames.insert(secondHalf);
        }
    });

    migrateCheckers(pg_db);
};

const migrateCheckers = async pg_db => {
    const new_sqlite_db = new sqlite3.Database('new_database.db');

    new_sqlite_db.all('SELECT instagramUsername, serverId, channelId, lastCode FROM instagramChecker;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_instagrams = rows.map(insta => {
                return {
                    'instagram_username': insta.instagramUsername,
                    'guild_id': insta.serverId,
                    'channel_id': insta.channelId,
                    'last_post_code': insta.lastCode
                };
            });

            await pg_db.checkers.instagram_checker.insert(pg_instagrams);
        }
    });

    new_sqlite_db.all('SELECT guildId, channelId, subreddit, lastLink, lastCreated FROM redditChecker;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_reddits = rows.map(reddit => {
                return {
                    'guild_id': reddit.guildId,
                    'channel_id': reddit.channelId,
                    'subreddit': reddit.subreddit,
                    'last_post_id': reddit.lastLink,
                    'last_created': reddit.lastCreated
                };
            });

            await pg_db.checkers.reddit_checker.insert(pg_reddits);
        }
    });

    new_sqlite_db.all('SELECT guildId, channelId, tumblrUser, lastLink FROM tumblrChecker;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_tumblrs = rows.map(tumblr => {
                return {
                    'guild_id': tumblr.guildId,
                    'channel_id': tumblr.channelId,
                    'tumblr_user': tumblr.tumblrUser,
                    'last_link': tumblr.lastLink
                };
            });

            await pg_db.checkers.tumblr_checker.insert(pg_tumblrs);
        }
    });

    new_sqlite_db.all('SELECT guildId, channelId, playlistId, lastLink FROM youtubeChecker;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_youtubes = rows.map(youtube => {
                return {
                    'guild_id': youtube.guildId,
                    'channel_id': youtube.channelId,
                    'playlist_id': youtube.playlistId,
                    'last_video_id': youtube.lastLink
                };
            });

            await pg_db.checkers.youtube_checker.insert(pg_youtubes);
        }
    });
};

// Needs to be done after first boot up, for the groups to be inserted.
const migrateRoles = async () => {
    const pg_db = await massive(PostgreSQLConfig);

    const sqlite_db = new sqlite3.Database('old_database.db');

    sqlite_db.all('SELECT id, modOf FROM modRole;', async (err, rows) => {
        if (err) {
            throw err;
        }
        else {
            const pg_mods = rows.map(mod => {
                return {
                    'guild_id': mod.modOf,
                    'role_id': mod.id,
                    'group_name': 'Moderators'
                };
            });

            await pg_db.guild_role_groups.insert(pg_mods);
        }
    });
};

const arg = process.argv[2];
if (!arg)
    migrate();
else if (arg === 'roles')
    migrateRoles();