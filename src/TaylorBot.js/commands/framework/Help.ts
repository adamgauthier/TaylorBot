'use strict';

import Command = require('../Command.js');
import CommandMessageContext = require('../CommandMessageContext.js');
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import ArrayUtil = require('../../modules/ArrayUtil.js');
import UserGroups = require('../../client/UserGroups.js');
import { CachedCommand } from '../../client/registry/CachedCommand.js';

class HelpCommand extends Command {
    constructor() {
        super({
            name: 'help',
            aliases: ['command'],
            group: 'framework',
            description: 'Provides help for a command.',
            examples: ['avatar', 'uinfo'],

            args: [
                {
                    key: 'command',
                    label: 'command',
                    type: 'command-or-itself',
                    prompt: 'What command would you like to get help for?'
                }
            ]
        });
    }

    run(commandContext: CommandMessageContext, { command }: { command: CachedCommand }): Promise<void> {
        const { message: { channel, author }, client, messageContext } = commandContext;

        if (command.command.name === commandContext.command.command.name) {
            const FEATURED_GROUPS = [
                { name: 'discord', emoji: '💬' },
                { name: 'fun', emoji: '🎭' },
                { name: 'knowledge', emoji: '❓' },
                { name: 'media', emoji: '📷' },
                { name: 'points', emoji: '💰' },
                { name: 'random', emoji: '🎲' },
                { name: 'reminders', emoji: '⏰' },
                { name: 'stats', emoji: '📊' },
                { name: 'weather', emoji: '🌦' }
            ];

            const groupedCommands = ArrayUtil.groupBy(
                client.master.registry.commands.getAllCommands().filter((c: CachedCommand) =>
                    FEATURED_GROUPS.map(group => group.name).includes(c.command.group) &&
                    c.command.minimumGroup !== UserGroups.Master
                ),
                (c: CachedCommand) => `${c.command.group} ${FEATURED_GROUPS.find(group => group.name === c.command.group)?.emoji}`
            );

            const embed = DiscordEmbedFormatter.baseUserEmbed(author)
                .setDescription(`Here are some featured commands, use \`${commandContext.usage()}\` for any of them. 😊`);

            for (const [group, groupCommands] of groupedCommands.entries()) {
                embed.addField(group, groupCommands.map(
                    (c: CachedCommand) => c.command.name
                ).join(', '), true);
            }

            return client.sendEmbed(channel, embed);
        }
        else {
            const helpCommandContext = new CommandMessageContext(messageContext, command);

            return client.sendEmbed(channel,
                DiscordEmbedFormatter
                    .baseUserEmbed(author)
                    .setTitle(`Command '${command.name}'`)
                    .setDescription(command.command.description)
                    .addField('Usage', [
                        `\`${helpCommandContext.usage()}\``,
                        ...helpCommandContext.argsUsage().map(({ identifier, hint }: { identifier: string; hint: string }) => `\`${identifier}\`: ${hint}`)
                    ].join('\n'))
                    .addField('Example', `\`${helpCommandContext.example()}\``)
            );
        }
    }
}

export = HelpCommand;
