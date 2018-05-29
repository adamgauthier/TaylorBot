'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

class EnableGuildCommandCommand extends Command {
    constructor() {
        super({
            name: 'enableguildcommand',
            aliases: ['enableservercommand', 'egc', 'esc'],
            group: 'admin',
            description: 'Enables a disabled command in a server.',
            minimumGroup: UserGroups.Master,
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
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { command, guild }) {
        if (!command.disabledIn[guild.id]) {
            throw new CommandError(`Command '${command.name}' is already enabled in ${guild.name}.`);
        }

        await command.enableIn(guild);
        return client.sendEmbedSuccess(message.channel, `Successfully enabled '${command.name}' in ${guild.name}.`);
    }
}

module.exports = EnableGuildCommandCommand;