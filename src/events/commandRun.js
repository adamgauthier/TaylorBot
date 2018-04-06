'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

class CommandRun extends EventHandler {
    handler({ oldRegistry }, command, promise, { message, argString }) {
        const { author, channel } = message;
        let logMessage = `${Format.user(author)} using '${command.name}' with args '${argString}' in ${Format.guildChannel(channel)}`;
        if (channel.type === 'text')
            logMessage += ` on ${Format.guild(message.guild)}`;
        Log.verbose(`${logMessage}.`);

        const commandTime = new Date().getTime();
        oldRegistry.users.updateLastCommand(author, commandTime);

        const final = () => {
            const answeredTime = new Date().getTime();
            oldRegistry.users.updateLastAnswered(author, answeredTime);
        };

        promise.then(final);
    }
}

module.exports = CommandRun;