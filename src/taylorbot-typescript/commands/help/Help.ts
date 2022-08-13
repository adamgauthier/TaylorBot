import { Command } from '../Command';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { CommandMessageContext } from '../CommandMessageContext';

class HelpCommand extends Command {
    constructor() {
        super({
            name: 'help',
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
        const { message: { channel }, client, messageContext, author } = commandContext;

        const command = client.master.registry.commands.resolve(commandName);

        // Command is on another service or general help
        if (command === undefined || command.name === commandContext.command.command.name)
            return;

        const helpCommandContext = new CommandMessageContext(messageContext, command);

        await client.sendEmbed(channel,
            DiscordEmbedFormatter
                .baseUserSuccessEmbed(author)
                .setTitle(`Command '${command.name}'`)
                .setDescription(command.command.description)
                .addFields([
                    {
                        name: 'Usage',
                        value: [
                            `\`${helpCommandContext.usage()}\``,
                            ...helpCommandContext.argsUsage().map(({ identifier, hint }) => `\`${identifier}\`: ${hint}`)
                        ].join('\n')
                    },
                    { name: 'Example', value: `\`${helpCommandContext.example()}\`` }
                ])
        );
    }
}

export = HelpCommand;
