'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class EnableGlobalCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'enableglobal',
            aliases: ['eg'],
            group: 'admin',
            memberName: 'enableglobal',
            description: 'Enables a disabled command globally.',
            examples: ['enableglobal avatar', 'eg uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                }
            ]
        }, UserGroups.Master);
    }

    async run(message, { command }) {
        const { commands } = this.client.oldRegistry;
        const cachedCommand = commands.get(command.name);

        if (cachedCommand.isDisabled) {
            await commands.enableCommand(command);
            return this.client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully enabled '${command.name}' globally.`));
        }
        else {
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already enabled.`));
        }
    }
}

module.exports = EnableGlobalCommand;