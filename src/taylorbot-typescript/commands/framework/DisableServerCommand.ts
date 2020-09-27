import UserGroups = require('../../client/UserGroups');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';
import { Guild } from 'discord.js';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';

class DisableServerCommandCommand extends Command {
    constructor() {
        super({
            name: 'disableservercommand',
            aliases: ['disableguildcommand', 'dgc', 'dsc'],
            group: 'framework',
            description: 'Disables an enabled command in a server.',
            minimumGroup: UserGroups.Moderators,
            examples: ['avatar', 'uinfo'],
            guildOnly: true,

            args: [
                {
                    key: 'databaseCommand',
                    label: 'command',
                    type: 'database-command',
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

    async run({ message, client }: CommandMessageContext, { databaseCommand, guild }: { databaseCommand: DatabaseCommand; guild: Guild }): Promise<void> {
        if (databaseCommand.module_name.toLowerCase() === 'framework') {
            throw new CommandError(`Can't disable \`${databaseCommand.name}\` because it's a framework command.`);
        }

        await client.master.registry.commands.setGuildEnabled(guild, databaseCommand.name, false);
        await client.sendEmbedSuccess(message.channel, `Successfully disabled \`${databaseCommand.name}\` in ${guild.name}.`);
    }
}

export = DisableServerCommandCommand;
