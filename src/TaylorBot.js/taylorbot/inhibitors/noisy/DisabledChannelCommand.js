'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class DisabledChannelCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ client, message, prefix }, command) {
        const { channel } = message;

        if (channel.type === 'text') {
            const isCommandDisabledInChannel = await client.master.registry.channelCommands.isCommandDisabledInChannel(channel, command);

            if (isCommandDisabledInChannel) {
                Log.verbose(`Command '${command.name}' can't be used in ${Format.channel(channel)} because it is disabled.`);
                return [
                    `You can't use \`${command.name}\` because it is disabled in ${channel}.`,
                    `You can re-enable it by typing \`${prefix}ecc ${command.name}\`.`
                ].join('\n');
            }
        }

        return null;
    }
}

module.exports = DisabledChannelCommandInhibitor;