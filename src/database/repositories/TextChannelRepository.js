'use strict';

const { Paths } = require('globalobjects');

const Log = require('../../tools/Logger.js');
const Format = require(Paths.DiscordFormatter);

class TextChannelRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.guilds.text_channels.find({}, {
                fields: ['channel_id', 'guild_id']
            });
        }
        catch (e) {
            Log.error(`Getting all text: ${e}`);
            throw e;
        }
    }

    async getAllInGuild(guild) {
        try {
            return await this._db.guilds.text_channels.find(
                {
                    'guild_id': guild.id
                },
                {
                    fields: ['channel_id']
                }
            );
        }
        catch (e) {
            Log.error(`Getting all guild text channels for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    async getAllLogChannelsInGuild(guild) {
        try {
            return await this._db.guilds.text_channels.find(
                {
                    'guild_id': guild.id,
                    'is_logging': true
                }
            );
        }
        catch (e) {
            Log.error(`Getting all guild log channels for guild ${Format.guild(guild)}: ${e}`);
            throw e;
        }
    }

    mapChannelToDatabase(guildChannel) {
        return {
            'guild_id': guildChannel.guild.id,
            'channel_id': guildChannel.id
        };
    }

    async get(guildChannel) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.guilds.text_channels.findOne(databaseChannel);
        }
        catch (e) {
            Log.error(`Getting text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async add(guildChannel, registeredAt) {
        if (guildChannel.type !== 'text')
            throw new Error(`Can't add non text channel ${Format.guildChannel(guildChannel)} to text channels.`);

        const databaseChannel = this.mapChannelToDatabase(guildChannel);

        try {
            return await this._db.guilds.text_channels.insert({ ...databaseChannel, 'registered_at': registeredAt });
        }
        catch (e) {
            Log.error(`Adding text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async addMessagesCount(guildChannel, messageCount) {
        try {
            return await this._db.query(
                'UPDATE guilds.text_channels SET messages_count = messages_count + $1 WHERE guild_id = $2 AND channel_id = $3 RETURNING *;',
                [messageCount, guildChannel.guild.id, guildChannel.id]
            );
        }
        catch (e) {
            Log.error(`Adding ${messageCount} messages to text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async _setLogging(guildChannel, isLogging) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.guilds.text_channels.update(databaseChannel,
                {
                    'is_logging': isLogging
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Setting ${Format.guildChannel(guildChannel)} as logging channel: ${e}`);
            throw e;
        }
    }

    async setLogging(guildChannel) {
        return this._setLogging(guildChannel, true);
    }

    async removeLogging(guildChannel) {
        return this._setLogging(guildChannel, false);
    }
}

module.exports = TextChannelRepository;