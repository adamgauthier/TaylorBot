'use strict';

const { Events } = require('discord.js').Constants;

const EventHandler = require('../structures/EventHandler.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class ChannelCreate extends EventHandler {
    constructor() {
        super(Events.CHANNEL_CREATE);
    }

    async handler(client, channel) {
        Log.verbose(`Channel discovered ${Format.channel(channel)}.`);

        if (channel.type === 'text') {
            await client.master.database.textChannels.add(channel, Date.now());
            Log.info(`Added new text channel ${Format.guildChannel(channel)}.`);
        }
    }
}

module.exports = ChannelCreate;