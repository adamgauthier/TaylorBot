'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class DisableGlobalCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'disableglobal',
            aliases: ['dg'],
            group: 'admin',
            memberName: 'disableglobal',
            description: 'Disables a command globally.',
            examples: ['disableglobal avatar', 'dg uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to disable?'
                }
            ]
        }, UserGroups.Master);
    }

    async run(message, { command }) {
        const { commands } = this.client.oldRegistry;
        const cachedCommand = commands.getCommand(command.name);

        if (cachedCommand.isDisabled) {
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already disabled.`));
        }
        else {
            if (command.minimumGroup === UserGroups.Master) {
                return this.client.sendEmbed(message.channel,
                    EmbedUtil.error(`Can't disable '${command.name}' because it's a Master Command.`));
            }
            else {
                await cachedCommand.disable();
                return this.client.sendEmbed(message.channel,
                    EmbedUtil.success(`Successfully disabled '${command.name}' globally.`));
            }
        }
    }
}

module.exports = DisableGlobalCommand;