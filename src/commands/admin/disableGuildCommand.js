'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class DisableGuildCommandCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'disableguildcommand',
            aliases: ['disableservercommand', 'dgc', 'dsc'],
            group: 'admin',
            memberName: 'disableguildcommand',
            description: 'Disables an enabled command in a server.',
            examples: ['disableguildcommand avatar', 'esc uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command',
                    prompt: 'What command would you like to disable?'
                },
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to disable the command in?',
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
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already disabled in ${guild.name}.`));
        }
        else {
            await cachedCommand.disableIn(guild);
            return this.client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully disabled '${command.name}' in ${guild.name}.`));
        }
    }
}

module.exports = DisableGuildCommandCommand;