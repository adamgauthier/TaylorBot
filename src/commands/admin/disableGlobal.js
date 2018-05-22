'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);
const CommandError = require(Paths.CommandError);

class DisableGlobalCommand extends Command {
    constructor() {
        super({
            name: 'disableglobal',
            aliases: ['dg'],
            group: 'admin',
            description: 'Disables a command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['disableglobal avatar', 'dg uinfo'],

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
        if (command.isDisabled) {
            throw new CommandError(`Command '${command.name}' is already disabled.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable '${command.name}' because it's a Master Command.`);
        }

        await command.disableCommand();
        return client.sendEmbed(message.channel,
            EmbedUtil.success(`Successfully disabled '${command.name}' globally.`));
    }
}

module.exports = DisableGlobalCommand;