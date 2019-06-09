'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');
const TimeUtil = require('../../modules/TimeUtil.js');

class IgnoredInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }) {
        const { author } = message;

        const commandTime = Date.now();
        const ignoredUntil = await client.master.registry.users.getIgnoredUntil(author.id);

        if (ignoredUntil && commandTime < ignoredUntil) {
            return `They are ignored until ${TimeUtil.formatLog(ignoredUntil)}.`;
        }

        return null;
    }
}

module.exports = IgnoredInhibitor;
