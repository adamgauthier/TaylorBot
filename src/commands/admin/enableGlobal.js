'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);

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

        if (cachedCommand.isDisabled) {
            await cachedCommand.enable();
            return client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully enabled '${command.name}' globally.`));
        }
        else {
            return client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already enabled.`));
        }
    }
}

module.exports = EnableGlobalCommand;