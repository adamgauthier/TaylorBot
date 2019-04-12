'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');

class GuildOnlyInhibitor extends NoisyInhibitor {
    getBlockedMessage({ message }, command) {
        const { channel } = message;

        if (command.command.guildOnly && channel.type !== 'text') {
            return {
                ui: `You can't use \`${command.name}\` because it can only be used in a server.`,
                log: 'The command is marked as guild only.'
            };
        }

        return null;
    }
}

module.exports = GuildOnlyInhibitor;