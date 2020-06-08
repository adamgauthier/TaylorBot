import Command = require('../Command.js');
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
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

        // Command is on another service or general help
        if (command === undefined || command.name === commandContext.command.command.name)
            return;

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

export = HelpCommand;
