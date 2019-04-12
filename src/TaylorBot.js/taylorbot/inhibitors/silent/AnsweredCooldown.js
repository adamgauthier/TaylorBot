'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');

class AnsweredCooldownInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }) {
        const { author } = message;

        if (await client.master.registry.answeredCooldowns.isAnswered(author)) {
            return 'They have not been answered.';
        }

        return null;
    }
}

module.exports = AnsweredCooldownInhibitor;