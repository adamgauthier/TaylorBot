'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class CommandRun extends EventHandler {
    handler({ oldRegistry }, command, promise, { message, argString }) {
        const { author, channel } = message;
        Log.verbose(
            `${Format.user(author)} using '${command.name}' with args '${argString}' in ${
                channel.type === 'dm' ?
                    Format.dmChannel(channel) :
                    channel.type === 'group' ?
                        Format.groupChannel(channel) :
                        Format.guildChannel(channel, '#name (#id) on #gName (#gId)')
            }.`
        );

        const commandTime = new Date().getTime();
        oldRegistry.users.updateLastCommand(author, commandTime);

        const final = () => {
            const answeredTime = new Date().getTime();
            oldRegistry.users.updateLastAnswered(author, answeredTime);
        };

        promise.then(final).catch(final);
    }
}

module.exports = CommandRun;