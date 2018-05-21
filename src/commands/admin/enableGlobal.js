'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);
const CommandError = require(Paths.CommandError);

class EnableGlobalCommand extends Command {
    constructor() {
        super({
            name: 'enableglobal',
            aliases: ['eg'],
            group: 'admin',
            description: 'Enables a disabled command globally.',
            minimumGroup: UserGroups.Master,
            examples: ['enableglobal avatar', 'eg uinfo'],

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
        const { commands } = client.master.registry;
        const cachedCommand = commands.getCommand(command.name);

        if (!cachedCommand.isDisabled) {
            throw new CommandError(`Command '${command.name}' is already enabled.`);
        }

        await cachedCommand.enable();
        return client.sendEmbed(message.channel,
            EmbedUtil.success(`Successfully enabled '${command.name}' globally.`));
    }
}

module.exports = EnableGlobalCommand;