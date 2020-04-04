'use strict';

const Command = require('../Command.js');
const UserGroups = require('../../client/UserGroups.js');

class EnableChannelCommandCommand extends Command {
    constructor() {
        super({
            name: 'enablechannelcommand',
            aliases: ['ecc'],
            group: 'admin',
            description: 'Enables a disabled command in a channel.',
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
                    prompt: 'What channel would you like to enable the command in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { command, channel }) {
        await client.master.registry.channelCommands.enableCommandInChannel(channel, command);

        return client.sendEmbedSuccess(message.channel, `Successfully enabled \`${command.name}\` in ${channel}.`);
    }
}

module.exports = EnableChannelCommandCommand;