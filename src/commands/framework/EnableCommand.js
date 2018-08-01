'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

class EnableCommandCommand extends Command {
    constructor() {
        super({
            name: 'enablecommand',
            aliases: ['ec'],
            group: 'framework',
            description: 'Enables a disabled command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['enablecommand avatar', 'ec uinfo'],
            guarded: true,

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                }
            ]
        });
    }

    async run({ message, client }, { command }) {
        if (!command.isDisabled) {
            throw new CommandError(`Command '${command.name}' is already enabled.`);
        }

        await command.enableCommand();
        return client.sendEmbedSuccess(message.channel, `Successfully enabled '${command.name}' globally.`);
    }
}

module.exports = EnableCommandCommand;