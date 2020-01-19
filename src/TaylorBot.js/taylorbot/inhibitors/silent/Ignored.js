'use strict';

const moment = require('moment');

const SilentInhibitor = require('../SilentInhibitor.js');
const TimeUtil = require('../../modules/TimeUtil.js');

class IgnoredInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }) {
        const { author } = message;

        const ignoredUntil = await client.master.registry.users.getIgnoredUntil(author);

        const commandTime = moment();
        if (commandTime.isBefore(ignoredUntil)) {
            return `They are ignored until ${TimeUtil.formatLog(ignoredUntil.valueOf())}.`;
        }

        return null;
    }
}

module.exports = IgnoredInhibitor;
