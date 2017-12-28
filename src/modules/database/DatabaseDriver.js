'use strict';

const sqlite3 = require('sqlite3').verbose();
const massive = require('massive');
const path = require('path');

const GlobalPaths = require(path.join(__dirname, '..', '..', 'GlobalPaths'));

const Log = require(GlobalPaths.Logger);
const PostgreSQLConfig = require(GlobalPaths.PostgreSQLConfig);

class DatabaseDriver {
    constructor() {
        this._sqlite_db = new sqlite3.Database(path.join(__dirname, 'database.db'));
    }

    async load() {
        this._db = await massive(PostgreSQLConfig);
    }

    async getAllGuildSettings() {
        try {
            return await new Promise((resolve, reject) => {
                this._sqlite_db.all('SELECT * FROM server;', (err, rows) => {
                    if (err) reject(err);
                    else resolve(rows);
                });
            });
        }
        catch (e) {
            Log.error(`Getting all guild settings: ${e}`);
        }
    }

    async guildUserExists(member) {
        const { user: u, guild: g } = member;
        try {
            return await this._guildUserExists(u.id, g.id);
        }
        catch (e) {
            Log.error(`Getting UserByServer ${u.username} (${u.id}) for guild ${g.name} (${g.id}) : ${e}`);
        }
    }

    _guildUserExists(userId, guildId) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.get('SELECT `id` FROM userByServer WHERE `id` = ? AND `serverId` = ? LIMIT 1;', [userId, guildId], (err, row) => {
                if (err) reject(err);
                else if (!row) resolve(false);
                else resolve(true);
            });
        });
    }

    async userExists(user) {
        try {
            return await this._userExists(user.id);
        }
        catch (e) {
            Log.error(`Checking User ${user.username} (${user.id}): ${e}`);
        }
    }

    _userExists(userId) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.get('SELECT `id` FROM user WHERE `id` = ? LIMIT 1;', userId, (err, row) => {
                if (err) reject(err);
                else if (!row) resolve(false);
                else resolve(true);
            });
        });
    }

    async addNewUser(user) {
        try {
            await this._addNewGlobalUser(user.id);
            Log.info(`Sucessfully added Global User ${user.username} (${user.id})`);
            return await this.addNewUsername(user);
        }
        catch (e) {
            Log.error(`Adding Global User ${user.username} (${user.id}): ${e}`);
        }
    }

    async addNewMember(member) {
        try {
            await this.addNewUser(member.user);
            const result = await this._addNewUserByGuild(member.id, member.guild.id, member.joinedAt.getTime());
            Log.info(`Added New Member ${member.user.username} (${member.id}) in ${member.guild.name} (${member.guild.id})`);
            return result;
        }
        catch (e) {
            Log.error(`Adding Member ${member.user.username} (${member.id}): ${e}`);
        }
    }

    _addNewGlobalUser(userId) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.run('INSERT INTO user (`id`) VALUES (?);', userId, err => {
                if (err) reject(err);
                else resolve({ 'lastID': this.lastID, 'changes': this.changes });
            });
        });
    }

    _addNewUserByGuild(userId, guildId, joinedAt = new Date().getTime()) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.run('INSERT INTO userByServer (`id`, `serverId`, `firstJoinedAt`) VALUES (?, ?, ?);', [userId, guildId, joinedAt], err => {
                if (err) reject(err);
                else resolve({ 'lastID': this.lastID, 'changes': this.changes });
            });
        });
    }

    async addNewUsername(user, since, silent = false) {
        try {
            const lastUsername = await this._getLastUsername(user.id);

            if (lastUsername && lastUsername.username === user.username) {
                if (!silent)
                    Log.warn(`New Username is already saved for ${user.username} (${user.id})`);
            }
            else {
                const result = await this._addNewUsername(user.id, user.username, since);
                Log.info(`Added New Username for ${user.username} (${user.id}), old was ${lastUsername.username}`);
                return result;
            }
        }
        catch (e) {
            Log.error(`Adding New Username ${user.username} (${user.id}): ${e}`);
        }
    }

    _getLastUsername(userId) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.get('SELECT `username` FROM usernames WHERE `userId` = ? GROUP BY `userId` LIMIT 1;', userId, (err, row) => {
                if (err) reject(err);
                else resolve(row);
            });
        });
    }

    _addNewUsername(userId, newUsername, since = new Date().getTime()) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.run('INSERT INTO usernames (`userId`, `username`, `since`) VALUES (?, ?, ?);', [userId, newUsername, since], err => {
                if (err) reject(err);
                else resolve({ 'lastID': this.lastID, 'changes': this.changes });
            });
        });
    }

    async getInstagrams() {
        try {
            return await this._db.instagram_checker.find();
        }
        catch (e) {
            Log.error(`Getting Instagrams: ${e}`);
            return [];
        }
    }

    async getReddits() {
        try {
            return await this._db.reddit_checker.find();
        }
        catch (e) {
            Log.error(`Getting Reddits: ${e}`);
            return [];
        }
    }

    async getYoutubeChannels() {
        try {
            return await new Promise((resolve, reject) => {
                this._sqlite_db.all('SELECT `guildId`, `channelId`, `playlistId`, `lastLink` FROM youtubeChecker;', (err, rows) => {
                    if (err) reject(err);
                    else resolve(rows);
                });
            });
        }
        catch (e) {
            Log.error(`Getting Youtube Channels: ${e}`);
            return [];
        }
    }

    async getTumblrs() {
        try {
            return await this._db.tumblr_checker.find();
        }
        catch (e) {
            Log.error(`Getting Tumblrs: ${e}`);
            return [];
        }
    }

    async updateTumblr(tumblrUser, guildId, channelId, lastLink) {
        try {
            return await this._db.tumblr_checker.update(
                {
                    'tumblr_user': tumblrUser,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_link': lastLink
                }
            );
        }
        catch (e) {
            Log.error(`Updating Tumblr for guild ${guildId}, channel ${channelId}, user ${tumblrUser}: ${e}`);
            throw e;
        }
    }

    async updateYoutube(lastLink, playlistId, guildId) {
        try {
            return await new Promise((resolve, reject) => {
                this._sqlite_db.run('UPDATE youtubeChecker SET `lastLink` = ? WHERE guildId = ? AND playlistId = ?;', [lastLink, guildId, playlistId], err => {
                    if (err) reject(err);
                    else resolve({ 'lastID': this.lastID, 'changes': this.changes });
                });
            });
        }
        catch (e) {
            Log.error(`Updating Youtube for guild ${guildId} and playlistId ${playlistId}: ${e}`);
            throw e;
        }
    }

    async updateReddit(subreddit, guildId, channelId, lastLink, lastCreated) {
        try {
            return await this._db.reddit_checker.update(
                {
                    'subreddit': subreddit,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_link': lastLink,
                    'last_created': lastCreated
                }
            );
        }
        catch (e) {
            Log.error(`Updating Reddit for guild ${guildId}, channel ${channelId}, subreddit ${subreddit}: ${e}`);
            throw e;
        }
    }

    async updateInstagram(instagramUsername, guildId, channelId, lastCode) {
        try {
            return await this._db.instagram_checker.update(
                {
                    'instagram_username': instagramUsername,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_post_code': lastCode
                }
            );
        }
        catch (e) {
            Log.error(`Updating Instagram for guild ${guildId}, channel ${channelId}, username ${instagramUsername}: ${e}`);
            throw e;
        }
    }

    async updateMinutes(minutesToAdd, minimumLastSpoke, minutesForReward, pointsToAdd) {
        try {
            await this._updateMinutes(minutesToAdd, minimumLastSpoke);
            return await this._updateMinutesMilestone(minutesForReward, pointsToAdd);
        }
        catch (e) {
            Log.error(`Updating minutes: ${e}`);
            throw e;
        }
    }

    _updateMinutes(minutesToAdd, minimumLastSpoke) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.run("UPDATE userByServer SET `minutes` = `minutes`+? WHERE `lastSpoke` > ?;", [minutesToAdd, minimumLastSpoke], err => {
                if (err) reject(err);
                else resolve({ 'lastID': this.lastID, 'changes': this.changes });
            });
        });
    }

    _updateMinutesMilestone(minutesForReward, pointsToAdd) {
        return new Promise((resolve, reject) => {
            this._sqlite_db.run("UPDATE userByServer SET `minutesMilestone` = (`minutes`-(`minutes`%?)), `taypoints`=`taypoints`+? WHERE `minutes` >= `minutesMilestone`+?;", [minutesForReward, pointsToAdd, minutesForReward], err => {
                if (err) reject(err);
                else resolve({ 'lastID': this.lastID, 'changes': this.changes });
            });
        });
    }
}

module.exports = new DatabaseDriver();