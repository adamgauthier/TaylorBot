'use strict';

const { GlobalPaths } = require('globalobjects');

const Inhibitor = require(GlobalPaths.Inhibitor);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class GuildOnlyInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { author, channel } = message;

        if (command.command.info.guildOnly && channel.type !== 'text') {
            Log.verbose(`Command '${command.command.info.name}' can't be used by ${Format.user(author)} in ${Format.dmChannel(channel)} because it is marked as guild only.`);
            return true;
        }

        return false;
    }
}

module.exports = GuildOnlyInhibitor;