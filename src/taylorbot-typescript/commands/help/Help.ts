import Command = require('../Command.js');
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import ArrayUtil = require('../../modules/ArrayUtil.js');
import UserGroups = require('../../client/UserGroups.js');
import { CachedCommand } from '../../client/registry/CachedCommand';
import { CommandMessageContext } from '../CommandMessageContext';

class HelpCommand extends Command {
    constructor() {
        super({
            name: 'help',
            aliases: ['command'],
            group: 'Help',
            description: 'Provides help for a command.',
            examples: ['avatar', 'uinfo'],

            args: [
                {
                    key: 'commandName',
                    label: 'command',
                    type: 'existing-command-name-or-itself',
                    prompt: 'What command would you like to get help for?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { commandName }: { commandName: string }): Promise<void> {
        const { message: { channel, author }, client, messageContext } = commandContext;

        const command = client.master.registry.commands.resolve(commandName);

        // Command is on another service
        if (command === undefined)
            return;

        if (command.name === commandContext.command.command.name) {
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
                client.master.registry.commands.getAllCommands().filter(c =>
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

            await client.sendEmbed(channel, embed);
        }
        else {
            const helpCommandContext = new CommandMessageContext(messageContext, command);

            await client.sendEmbed(channel,
                DiscordEmbedFormatter
                    .baseUserEmbed(author)
                    .setTitle(`Command '${command.name}'`)
                    .setDescription(command.command.description)
                    .addField('Usage', [
                        `\`${helpCommandContext.usage()}\``,
                        ...helpCommandContext.argsUsage().map(({ identifier, hint }) => `\`${identifier}\`: ${hint}`)
                    ].join('\n'))
                    .addField('Example', `\`${helpCommandContext.example()}\``)
            );
        }
    }
}

export = HelpCommand;
