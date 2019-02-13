'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');

class AnsweredCooldownInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }, command) {
        const { author } = message;

        if (await client.master.registry.answeredCooldowns.isAnswered(author)) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have not been answered.`);
            return true;
        }

        return false;
    }
}

module.exports = AnsweredCooldownInhibitor;