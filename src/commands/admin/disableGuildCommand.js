'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);
const CommandError = require(Paths.CommandError);

class DisableGuildCommandCommand extends Command {
    constructor() {
        super({
            name: 'disableguildcommand',
            aliases: ['disableservercommand', 'dgc', 'dsc'],
            group: 'admin',
            description: 'Disables an enabled command in a server.',
            minimumGroup: UserGroups.Master,
            examples: ['disableguildcommand avatar', 'dsc uinfo'],

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
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { command, guild }) {
        if (command.disabledIn[guild.id]) {
            throw new CommandError(`Command '${command.name}' is already disabled in ${guild.name}.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable '${command.name}' because it's a Master Command.`);
        }

        await command.disableIn(guild);
        return client.sendEmbed(message.channel,
            EmbedUtil.success(`Successfully disabled '${command.name}' in ${guild.name}.`));
    }
}

module.exports = DisableGuildCommandCommand;