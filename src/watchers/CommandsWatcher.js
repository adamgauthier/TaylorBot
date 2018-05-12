'use strict';

const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);

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


    }
}

module.exports = CommandsWatcher;