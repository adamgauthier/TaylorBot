'use strict';

const Command = require('../Command.js');
const CommandMessageContext = require('../CommandMessageContext.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

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
                    type: 'command',
                    prompt: 'What command would you like to get help for?'
                }
            ]
        });
    }

    async run({ message: { channel, author }, client, messageContext }, { command }) {
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

module.exports = HelpCommand;