import UserGroups = require('../../client/UserGroups.js');
import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { Guild } from 'discord.js';

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

    async run({ message, client }: CommandMessageContext, { command, guild }: { command: CachedCommand; guild: Guild }): Promise<void> {
        const isDisabled = await client.master.registry.commands.getIsGuildCommandDisabled(guild, command);

        if (isDisabled) {
            throw new CommandError(`Command \`${command.name}\` is already disabled in ${guild.name}.`);
        }

        if (command.command.minimumGroup === UserGroups.Master) {
            throw new CommandError(`Can't disable \`${command.name}\` because it's a Master command.`);
        }

        if (command.command.group === 'framework') {
            throw new CommandError(`Can't disable \`${command.name}\` because it's a framework command.`);
        }

        await command.disableIn(guild);
        await client.sendEmbedSuccess(message.channel, `Successfully disabled \`${command.name}\` in ${guild.name}.`);
    }
}

export = DisableServerCommandCommand;
