import { CommandMessageContext } from '../CommandMessageContext';
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { TextChannel } from 'discord.js';

const OPTIONS_SYMBOLS = ['1⃣', '2⃣', '3⃣', '4⃣'];

class ReactPollCommand extends Command {
    constructor() {
        super({
            name: 'reactpoll',
            aliases: ['rpoll'],
            group: 'Fun 🎭',
            description: 'Creates a quick poll for a few options with reactions!',
            examples: ['Cake, Pie'],
            guildOnly: true,

            args: [
                {
                    key: 'options',
                    label: 'option1,option2,...',
                    type: 'poll-options',
                    prompt: 'What are the options (comma separated) for your poll?'
                }
            ]
        });
    }

    async run({ message, client, author, messageContext }: CommandMessageContext, { options }: { options: string[] }): Promise<void> {
        const channel = message.channel as TextChannel;
        if (options.length > OPTIONS_SYMBOLS.length) {
            throw new CommandError(`A react poll can't have more than ${OPTIONS_SYMBOLS.length} options. Use the \`${messageContext.prefix}poll\` command instead.`);
        }

        const pollOptions = options.reduce(
            (map, name, index) => map.set(OPTIONS_SYMBOLS[index], name),
            new Map<string, string>()
        );

        const pollMessage = await client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setTitle(`React Poll '${channel.name}' started!`)
            .setDescription(Array.from(
                pollOptions.entries(),
                ([key, option]) => `${key}: ${option}`
            ).join('\n'))
            .setFooter('React to vote!')
        );

        for (const key of pollOptions.keys()) {
            await pollMessage.react(key);
        }
    }
}

export = ReactPollCommand;
