'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const ArrayUtil = require(GlobalPaths.ArrayUtil);

class CommandsWatcher extends MessageWatcher {
    constructor() {
        super(false);
    }

    async messageHandler(client, message) {
        const { author } = message;
        if (author.bot)
            return;

        const { oldRegistry } = client;
        const { channel } = message;
        let text = message.content;
        if (channel.type === 'text') {
            const { guild } = message;

            const { prefix } = oldRegistry.guilds.get(guild.id);

            if (text.startsWith(prefix)) {
                text = text.substring(prefix.length);
            }
            else {
                return;
            }
        }

        const args = text.split(' ');
        const commandName = args.shift().toLowerCase();
        const command = oldRegistry.commands.getCommand(commandName);

        if (!command)
            return;

        const argString = args.join(' ');

        for (const inhibitor of oldRegistry.inhibitors.values()) {
            if (inhibitor.shouldBeBlocked(message, command)) {
                return;
            }
        }

        Log.verbose(
            `${Format.user(author)} using '${command.name}' with args '${argString}' in ${
                channel.type === 'dm' ?
                    Format.dmChannel(channel) :
                    Format.guildChannel(channel, '#name (#id) on #gName (#gId)')
            }.`
        );

        const regexString = command.args
            .map(argInfo => {
                return argInfo.quoted ? `(?:'|")(.*)(?:'|")` : '(.*)';
            })
            .join(command.separator);

        const regex = new RegExp(`^${regexString}$`);

        const matches = argString.trim().match(regex);

        if (!matches) {
            // SEND ERROR MESSAGE
            return;
        }

        const parsedArgs = {};

        for (const [match, argInfo] of ArrayUtil.iterateArrays(matches, command.args)) {
            const type = oldRegistry.getType(argInfo.type);
            if (type.isEmpty(match)) {
                // SEND ERROR MESSAGE
                return;
            }

            try {
                const parsedArg = await type.parse(match, message, argInfo);

                parsedArgs[argInfo.key] = parsedArg;
            }
            catch (e) {
                // UPDATE ANSWERED

                if (e instanceof ArgumentParsingError) {
                    // SEND ERROR MESSAGE argInfo.label
                    return;
                }
                else {
                    // MEMES
                    throw e;
                }
            }
        }

        try {
            command.run(message, args);
        }
        catch (e) {
            if (e instanceof CommandError) {
                // SEND ERROR MESSAGE
                return;
            }
            else {
                // SEND UNKNOWN ERROR MESSAGE
                throw e;
            }
        }
        finally {
            // UPDATE ANSWERED
        }
    }
}

module.exports = CommandsWatcher;