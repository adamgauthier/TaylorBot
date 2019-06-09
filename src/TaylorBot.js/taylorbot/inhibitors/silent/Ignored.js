'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');
const TimeUtil = require('../../modules/TimeUtil.js');

class IgnoredInhibitor extends SilentInhibitor {
    shouldBeBlocked({ message, client }) {
        const { author } = message;

        const commandTime = Date.now();
        const cachedUser = client.master.registry.users.get(author.id);

        if (cachedUser && commandTime < cachedUser.ignoreUntil) {
            return `They are ignored until ${TimeUtil.formatLog(cachedUser.ignoreUntil)}.`;
        }

        return null;
    }
}

module.exports = IgnoredInhibitor;
