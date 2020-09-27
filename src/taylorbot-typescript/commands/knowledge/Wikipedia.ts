import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { WikipediaModule } from '../../modules/wiki/WikipediaModule';
import { CommandMessageContext } from '../CommandMessageContext';

class WikipediaCommand extends Command {
    constructor() {
        super({
            name: 'wikipedia',
            aliases: ['wiki'],
            group: 'Knowledge ❓',
            description: 'Search on Wikipedia!',
            examples: ['taylor swift', 'canada'],
            maxDailyUseCount: 100,

            args: [
                {
                    key: 'title',
                    label: 'title',
                    type: 'text',
                    prompt: 'What Wikipedia article would you like to see?'
                }
            ]
        });
    }

    async run({ message, client, author }: CommandMessageContext, { title }: { title: string }): Promise<void> {
        const { channel } = message;

        const page = await WikipediaModule.getPage(title);
        if (page.invalid)
            throw new CommandError(`Article title '${title}' is invalid.`);

        if (page.missing)
            throw new CommandError(`Could not find Wikipedia article for '${title}'.`);

        await client.sendEmbed(channel, WikipediaModule.getPageEmbed(author, page));
    }
}

export = WikipediaCommand;
