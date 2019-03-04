'use strict';

const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class ChannelCommandRepository {
    constructor(db) {
        this._db = db;
    }

    async getAll() {
        try {
            return await this._db.any('SELECT * FROM guilds.channel_commands;');
        }
        catch (e) {
            Log.error(`Getting all channel commands: ${e}`);
            throw e;
        }
    }

    async disableCommandInChannel(guildTextChannel, command) {
        try {
            return await this._db.none(
                `INSERT INTO guilds.channel_commands (guild_id, channel_id, command_id)
                VALUES ($[guild_id], $[channel_id], $[command_id]) ON CONFLICT DO NOTHING;`,
                {
                    guild_id: guildTextChannel.guild.id,
                    channel_id: guildTextChannel.id,
                    command_id: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Disabling channel command '${command.name}' for channel ${Format.channel(guildTextChannel)}: ${e}`);
            throw e;
        }
    }

    async enableCommandInChannel(guildTextChannel, command) {
        try {
            return await this._db.none(
                'DELETE FROM guilds.channel_commands WHERE guild_id = $[guild_id] AND channel_id = $[channel_id] AND command_id = $[command_id];',
                {
                    guild_id: guildTextChannel.guild.id,
                    channel_id: guildTextChannel.id,
                    command_id: command.name
                }
            );
        }
        catch (e) {
            Log.error(`Enabling channel command '${command.name}' for channel ${Format.channel(guildTextChannel)}: ${e}`);
            throw e;
        }
    }
}

module.exports = ChannelCommandRepository;