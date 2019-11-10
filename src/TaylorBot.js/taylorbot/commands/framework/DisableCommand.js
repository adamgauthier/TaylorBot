'use strict';

const UserGroups = require('../../client/UserGroups.js');
const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class DisableCommandCommand extends Command {
    constructor() {
        super({
            name: 'disablecommand',
            aliases: ['dc'],
            group: 'framework',
            description: 'Disables a command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['avatar', 'uinfo'],
            guarded: true,

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to disable?'
                }
            ]
        });
    }

    async run({ message, client }, { command }) {
        const isDisabled = await client.master.registry.commands.insertOrGetIsCommandDisabled(command);

        if (isDisabled) {
            throw new CommandError(`Command '${command.name}' is already disabled.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable '${command.name}' because it's a Master command.`);
        }

        if (command.command.guarded) {
            throw new CommandError(`Can't disable '${command.name}' because it's guarded.`);
        }

        await command.disableCommand();
        return client.sendEmbedSuccess(message.channel, `Successfully disabled '${command.name}' globally.`);
    }
}

module.exports = DisableCommandCommand;
