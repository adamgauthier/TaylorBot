'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const UserGroups = require('../../client/UserGroups.js');

class DisableChannelCommandCommand extends Command {
    constructor() {
        super({
            name: 'disablechannelcommand',
            aliases: ['dcc'],
            group: 'admin',
            description: 'Disables a command in a channel.',
            minimumGroup: UserGroups.Moderators,
            examples: ['roll general', 'gamble'],
            guildOnly: true,
            guarded: true,

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                },
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like to disable the command in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { command, channel }) {
        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable \`${command.name}\` because it's a Master command.`);
        }

        if (command.command.guarded) {
            throw new CommandError(`Can't disable \`${command.name}\` because it's guarded.`);
        }

        await client.master.registry.channelCommands.disableCommandInChannel(channel, command);

        return client.sendEmbedSuccess(message.channel, `Successfully disabled \`${command.name}\` in ${channel}.`);
    }
}

module.exports = DisableChannelCommandCommand;