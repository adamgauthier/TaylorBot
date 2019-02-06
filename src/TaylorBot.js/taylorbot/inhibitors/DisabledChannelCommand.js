'use strict';

const Inhibitor = require('../structures/Inhibitor.js');
const Log = require('../tools/Logger.js');
const Format = require('../modules/DiscordFormatter.js');

class DisabledChannelCommandInhibitor extends Inhibitor {
    async shouldBeBlocked({ client, message }, command) {
        const { guild, channel } = message;

        if (!guild)
            return false;

        const isCommandDisabledInChannel = await client.master.registry.channelCommands.isCommandDisabledInChannel(channel, command);

        if (isCommandDisabledInChannel) {
            Log.verbose(`Command '${command.name}' can't be used in ${Format.channel(channel)} because it is disabled.`);
            return true;
        }

        return false;
    }
}

module.exports = DisabledChannelCommandInhibitor;