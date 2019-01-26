'use strict';

const Command = require('../Command.js');
const CommandMessageContext = require('../CommandMessageContext.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');
const UserGroups = require('../../client/UserGroups.js');

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

    run(commandContext, { command }) {
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
                client.master.registry.commands.getAllCommands().filter(
                    c => FEATURED_GROUPS.map(group => group.name).includes(c.command.group) &&
                        c.command.minimumGroup !== UserGroups.Master
                ),
                c => `${c.command.group} ${FEATURED_GROUPS.find(group => group.name === c.command.group).emoji}`
            );

            const embed = DiscordEmbedFormatter.baseUserEmbed(author)
                .setDescription(`Here are some featured commands, use \`${commandContext.usage()}\` for any of them. 😊`);

            for (const [group, groupCommands] of groupedCommands.entries()) {
                embed.addField(group, groupCommands.map(
                    c => c.command.name
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
                        ...helpCommandContext.argsUsage().map(({ identifier, hint }) => `\`${identifier}\`: ${hint}`)
                    ].join('\n'))
                    .addField('Example', `\`${helpCommandContext.example()}\``)
            );
        }
    }
}

module.exports = HelpCommand;