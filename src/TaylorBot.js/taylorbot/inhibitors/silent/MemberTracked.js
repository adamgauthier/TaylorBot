'use strict';

const SilentInhibitor = require('../SilentInhibitor.js');
const Format = require('../../modules/DiscordFormatter.js');
const Log = require('../../tools/Logger.js');

class MemberTracked extends SilentInhibitor {
    async shouldBeBlocked({ message, client }) {
        const { member } = message;

        if (!member)
            return null;

        const memberAdded = await client.master.registry.guilds.addOrUpdateMemberAsync(member);

        if (memberAdded) {
            Log.verbose(`Added new member ${Format.member(member)}.`);
        }

        return null;
    }
}

module.exports = MemberTracked;
