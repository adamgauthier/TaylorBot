'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

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
                    'is_log': true
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

    async addMessages(guildChannel, messagesToAdd) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.instance.one([
                'UPDATE guilds.text_channels',
                'SET message_count = message_count + ${messages_to_add}',
                'WHERE guild_id = ${guild_id} AND channel_id = ${channel_id}',
                'RETURNING *;'
            ].join('\n'),
            {
                ...databaseChannel,
                messages_to_add: messagesToAdd
            });
        }
        catch (e) {
            Log.error(`Adding ${messagesToAdd} messages to text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async removeMessages(guildChannel, messagesToRemove) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.instance.one([
                'UPDATE guilds.text_channels',
                'SET message_count = GREATEST(0, message_count - ${messages_to_remove})',
                'WHERE guild_id = ${guild_id} AND channel_id = ${channel_id}',
                'RETURNING *;'
            ].join('\n'),
            {
                ...databaseChannel,
                messages_to_remove: messagesToRemove
            });
        }
        catch (e) {
            Log.error(`Removing ${messagesToRemove} messages from text channel ${Format.guildChannel(guildChannel)}: ${e}`);
            throw e;
        }
    }

    async _setLog(guildChannel, isLog) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.guilds.text_channels.update(databaseChannel,
                {
                    'is_log': isLog
                },
                {
                    'single': true
                }
            );
        }
        catch (e) {
            Log.error(`Setting ${Format.guildChannel(guildChannel)} as log channel: ${e}`);
            throw e;
        }
    }

    setLog(guildChannel) {
        return this._setLog(guildChannel, true);
    }

    removeLog(guildChannel) {
        return this._setLog(guildChannel, false);
    }

    async _setSpam(guildChannel, isSpam) {
        const databaseChannel = this.mapChannelToDatabase(guildChannel);
        try {
            return await this._db.instance.one([
                'UPDATE guilds.text_channels',
                'SET is_spam = ${is_spam}',
                'WHERE guild_id = ${guild_id} AND channel_id = ${channel_id}',
                'RETURNING *;'
            ].join('\n'),
            {
                ...databaseChannel,
                'is_spam': isSpam
            });
        }
        catch (e) {
            Log.error(`Setting ${Format.guildChannel(guildChannel)} as spam channel: ${e}`);
            throw e;
        }
    }

    setSpam(guildChannel) {
        return this._setSpam(guildChannel, true);
    }

    removeSpam(guildChannel) {
        return this._setSpam(guildChannel, false);
    }
}

module.exports = TextChannelRepository;