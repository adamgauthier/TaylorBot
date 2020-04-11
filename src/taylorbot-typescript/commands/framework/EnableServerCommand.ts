import UserGroups = require('../../client/UserGroups.js');
import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { Guild } from 'discord.js';

class EnableServerCommandCommand extends Command {
    constructor() {
        super({
            name: 'enableservercommand',
            aliases: ['enableguildcommand', 'egc', 'esc'],
            group: 'framework',
            description: 'Enables a disabled command in a server.',
            minimumGroup: UserGroups.Moderators,
            examples: ['avatar', 'uinfo'],
            guildOnly: true,

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

    async run({ message, client }: CommandMessageContext, { command, guild }: { command: CachedCommand; guild: Guild }): Promise<void> {
        const isDisabled = await client.master.registry.commands.getIsGuildCommandDisabled(guild, command);

        if (!isDisabled) {
            throw new CommandError(`Command \`${command.name}\` is already enabled in ${guild.name}.`);
        }

        await command.enableIn(guild);
        await client.sendEmbedSuccess(message.channel, `Successfully enabled \`${command.name}\` in ${guild.name}.`);
    }
}

export = EnableServerCommandCommand;
