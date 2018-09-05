'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');

class DisableGuildCommandCommand extends Command {
    constructor() {
        super({
            name: 'disableguildcommand',
            aliases: ['disableservercommand', 'dgc', 'dsc'],
            group: 'admin',
            description: 'Disables an enabled command in a server.',
            minimumGroup: UserGroups.Moderators,
            examples: ['avatar', 'uinfo'],
            guildOnly: true,
            guarded: true,

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
                    type: 'guild-or-current',
                    prompt: 'What server would you like to disable the command in?'
                }
            ]
        });
    }

    async run({ message, client }, { command, guild }) {
        if (command.disabledIn[guild.id]) {
            throw new CommandError(`Command '${command.name}' is already disabled in ${guild.name}.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable '${command.name}' because it's a Master command.`);
        }

        if (command.command.guarded) {
            throw new CommandError(`Can't disable '${command.name}' because it's guarded.`);
        }

        await command.disableIn(guild);
        return client.sendEmbedSuccess(message.channel, `Successfully disabled '${command.name}' in ${guild.name}.`);
    }
}

module.exports = DisableGuildCommandCommand;