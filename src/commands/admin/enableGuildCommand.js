'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class EnableGuildCommandCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'enableguildcommand',
            aliases: ['enableservercommand', 'egc', 'esc'],
            group: 'admin',
            memberName: 'enableguildcommand',
            description: 'Enables a disabled command in a server.',
            examples: ['enableguildcommand avatar', 'esc uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to enable?'
                },
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to enable the command in?',
                    type: 'guild-or-current',
                    error: 'Could not find server'
                }
            ]
        }, UserGroups.Master);
    }

    async run(message, { command, guild }) {
        const { commands } = this.client.oldRegistry;
        const cachedCommand = commands.get(command.name);

        if (cachedCommand.disabledIn[guild.id]) {
            await commands.enableCommandIn(command, guild);
            return this.client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully enabled '${command.name}' in ${guild.name}.`));
        }
        else {
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already enabled in ${guild.name}.`));
        }
    }
}

module.exports = EnableGuildCommandCommand;