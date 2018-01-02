'use strict';

const sqlite3 = require('sqlite3').verbose();
const massive = require('massive');
const path = require('path');

const GlobalPaths = require(path.join(__dirname, '..', '..', 'GlobalPaths'));

const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const PostgreSQLConfig = require(GlobalPaths.PostgreSQLConfig);

class DatabaseDriver {
    constructor() {
        this._sqlite_db = new sqlite3.Database(path.join(__dirname, 'database.db'));
    }

    async load() {
        this._db = await massive(PostgreSQLConfig);
    }

    async getAllGuilds() {
        try {
            return await this._db.guilds.find();
        }
        catch (e) {
            Log.error(`Getting all guilds: ${e}`);
            throw e;
        }
    }

    mapGuildToDatabase(guild) {
        return {
            'guild_id': guild.id
        };
    }

    async getGuild(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.findOne(databaseGuild);
        }
        catch (e) {
            Log.error(`Getting guild ${Format.formatGuild(guild)}: ${e}`);
            throw e;
        }
    }

    async addGuild(guild) {
        const databaseGuild = this.mapGuildToDatabase(guild);
        try {
            return await this._db.guilds.insert(databaseGuild);
        }
        catch (e) {
            Log.error(`Adding guild ${Format.formatGuild(guild)}: ${e}`);
            throw e;
        }
    }

    async getAllUsers() {
        try {
            return await this._db.users.find({}, {
                fields: ['user_id', 'last_command', 'last_answered', 'ignore_until']
            });
        }
        catch (e) {
            Log.error(`Getting all users: ${e}`);
            throw e;
        }
    }

    mapUserToDatabase(user) {
        return {
            'user_id': user.id
        };
    }

    async getUser(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.findOne(databaseUser);
        }
        catch (e) {
            Log.error(`Getting user ${Format.formatUser(user)}: ${e}`);
            throw e;
        }
    }

    async addUser(user) {
        const databaseUser = this.mapUserToDatabase(user);
        try {
            return await this._db.users.insert(databaseUser);
        }
        catch (e) {
            Log.error(`Adding user ${Format.formatUser(user)}: ${e}`);
            throw e;
        }
    }

    async getAllGuildMembers() {
        try {
            return await this._db.guild_members.find({}, {
                fields: ['user_id', 'guild_id']
            });
        }
        catch (e) {
            Log.error(`Getting all guild members: ${e}`);
            throw e;
        }
    }

    mapMemberToDatabase(guildMember) {
        return {
            'guild_id': guildMember.guild.id,
            'user_id': guildMember.id,
            'first_joined_at': guildMember.joinedTimestamp
        }
    }

    async addGuildMember(guildMember) {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            return await this._db.guild_members.insert(databaseMember);
        }
        catch (e) {
            Log.error(`Adding member ${Format.formatMember(guildMember)}: ${e}`);
            throw e;
        }
    }

    async guildUserExists(member) {
        const { user: u, guild: g } = member;
        try {
            return await this._guildUserExists(u.id, g.id);
        }
        catch (e) {
            Log.error(`Getting UserByServer ${Format.formatUser(u)} for guild ${Format.formatGuild(g)} : ${e}`);
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
            Log.error(`Checking User ${Format.formatUser(user)}: ${e}`);
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
            Log.info(`Sucessfully added Global User ${Format.formatUser(user)}`);
            return await this.addNewUsername(user);
        }
        catch (e) {
            Log.error(`Adding Global User ${Format.formatUser(user)}: ${e}`);
        }
    }

    async addNewMember(member) {
        const { user, guild } = member;
        try {
            await this.addNewUser(user);
            const result = await this._addNewUserByGuild(user.id, guild.id, member.joinedAt.getTime());
            Log.info(`Added New Member ${Format.formatUser(user)} in ${Format.formatGuild(guild)}`);
            return result;
        }
        catch (e) {
            Log.error(`Adding Member ${Format.formatUser(user)}: ${e}`);
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
                    Log.warn(`New Username is already saved for ${Format.formatUser(user)}`);
            }
            else {
                const result = await this._addNewUsername(user.id, user.username, since);
                Log.info(`Added New Username for ${Format.formatUser(user)}, old was ${lastUsername.username}`);
                return result;
            }
        }
        catch (e) {
            Log.error(`Adding New Username ${Format.formatUser(user)}: ${e}`);
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
            return await this._db.youtube_checker.find();
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

    async updateYoutube(playlistId, guildId, channelId, lastLink) {
        try {
            return await this._db.youtube_checker.update(
                {
                    'playlist_id': playlistId,
                    'guild_id': guildId,
                    'channel_id': channelId
                },
                {
                    'last_link': lastLink
                }
            );
        }
        catch (e) {
            Log.error(`Updating Youtube for guild ${guildId}, channel ${channelId}, playlistId ${playlistId}: ${e}`);
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