'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class ProRepository {
    constructor(db) {
        this._db = db;
    }

    async proGuildExists(guild) {
        try {
            return await this._db.one(
                'SELECT (COUNT(*) > 0) AS guild_exists FROM guilds.pro_guilds WHERE guild_id = $[guild_id];',
                {
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting pro guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getUser(user) {
        try {
            return await this._db.oneOrNone(
                'SELECT * FROM users.pro_users WHERE user_id = $[user_id];',
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Getting pro user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async countUserProGuilds(user) {
        try {
            return await this._db.one(
                'SELECT COUNT(*) FROM guilds.pro_guilds WHERE pro_user_id = $[user_id];',
                {
                    user_id: user.id
                }
            );
        }
        catch (e) {
            Log.error(`Counting pro guilds for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async addUserProGuild(user, guild) {
        try {
            return await this._db.none(
                'INSERT INTO guilds.pro_guilds (guild_id, pro_user_id) VALUES ($[guild_id], $[user_id]);',
                {
                    user_id: user.id,
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Adding pro guild ${Format.guild(guild)} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }

    async removeUserProGuild(user, guild) {
        try {
            return await this._db.none(
                'DELETE FROM guilds.pro_guilds WHERE guild_id = $[guild_id] AND pro_user_id = $[user_id];',
                {
                    user_id: user.id,
                    guild_id: guild.id
                }
            );
        }
        catch (e) {
            Log.error(`Removing pro guild ${Format.guild(guild)} for user ${Format.user(user)}: ${e}`);
            throw e;
        }
    }
}

module.exports = ProRepository;