'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class GuildOnlyInhibitor extends NoisyInhibitor {
    getBlockedMessage({ message }, command) {
        const { author, channel } = message;

        if (command.command.guildOnly && channel.type !== 'text') {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} in ${Format.channel(channel)} because it is marked as guild only.`);
            return `You can't use \`${command.name}\` because it can only be used in a server.`;
        }

        return null;
    }
}

module.exports = GuildOnlyInhibitor;