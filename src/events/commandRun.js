'use strict';

const { GlobalPaths } = require('globalobjects');

const EventHandler = require(GlobalPaths.EventHandler);
const Log = require(GlobalPaths.Logger);

class CommandRun extends EventHandler {
    handler({ oldRegistry }, command, promise, { message, argString }) {
        let logMessage = `${Format.user(message.author)} attempting to use '${command.name}' with args '${argString}' in ${Format.guildChannel(message.channel)}`;
        if (message.channel.type === 'text')
            logMessage += ` on ${Format.guild(message.guild)}`;
        Log.verbose(`${logMessage}.`);

        const commandTime = new Date().getTime();
        oldRegistry.users.updateLastCommand(message.author, commandTime);

        const { lastCommand, lastAnswered, ignoreUntil } = oldRegistry.users.get(author.id);

        if (commandTime < ignoreUntil) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they are ignored until ${moment(ignoreUntil, 'x').format('MMM Do YY, H:mm:ss Z')}.`);
            return;
        }

        if (lastAnswered < lastCommand) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
            return;
        }

        promise.finally(() => {
            const answeredTime = new Date().getTime();
            oldRegistry.users.updateLastAnswered(author, answeredTime);
        });
    }
}

module.exports = CommandRun;