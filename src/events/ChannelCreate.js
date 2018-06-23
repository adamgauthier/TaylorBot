'use strict';

const { Events } = require('discord.js').Constants;
const { Paths } = require('globalobjects');

const EventHandler = require(Paths.EventHandler);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

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