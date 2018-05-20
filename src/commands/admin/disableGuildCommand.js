'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);

class DisableGuildCommandCommand extends Command {
    constructor() {
        super({
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

    async run({ message, client }, { command, guild }) {
        const { commands } = client.master.oldRegistry;
        const cachedCommand = commands.get(command.name);

        if (cachedCommand.disabledIn[guild.id]) {
            return client.sendEmbed(message.channel,
                EmbedUtil.error(`Command '${command.name}' is already disabled in ${guild.name}.`));
        }
        else {
            await cachedCommand.disableIn(guild);
            return client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully disabled '${command.name}' in ${guild.name}.`));
        }
    }
}

module.exports = DisableGuildCommandCommand;