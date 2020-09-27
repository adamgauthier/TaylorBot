import UserGroups = require('../../client/UserGroups');
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';
import { Guild } from 'discord.js';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';

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
                    key: 'databaseCommand',
                    label: 'command',
                    type: 'database-command',
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

    async run({ message, client }: CommandMessageContext, { databaseCommand, guild }: { databaseCommand: DatabaseCommand; guild: Guild }): Promise<void> {
        await client.master.registry.commands.setGuildEnabled(guild, databaseCommand.name, true);
        await client.sendEmbedSuccess(message.channel, `Successfully enabled \`${databaseCommand.name}\` in ${guild.name}.`);
    }
}

export = EnableServerCommandCommand;
