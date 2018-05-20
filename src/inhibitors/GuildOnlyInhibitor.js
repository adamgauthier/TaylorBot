'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);

class GuildOnlyInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { author, channel } = message;

        if (command.command.guildOnly && channel.type !== 'text') {
            Log.verbose(`Command '${command.command.name}' can't be used by ${Format.user(author)} in ${Format.dmChannel(channel)} because it is marked as guild only.`);
            return true;
        }

        return false;
    }
}

module.exports = GuildOnlyInhibitor;