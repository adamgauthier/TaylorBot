'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');
const TimeUtil = require('../../modules/TimeUtil.js');

class IgnoredInhibitor extends SilentInhibitor {
    shouldBeBlocked({ message, client }) {
        const { author } = message;

        const commandTime = Date.now();
        const { ignoreUntil } = client.master.registry.users.get(author.id);

        if (commandTime < ignoreUntil) {
            return `They are ignored until ${TimeUtil.formatLog(ignoreUntil)}.`;
        }

        return null;
    }
}

module.exports = IgnoredInhibitor;