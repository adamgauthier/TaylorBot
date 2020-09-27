import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { UrbanDictionaryModule } from '../../modules/urban/UrbanDictionaryModule';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { PageMessage } from '../../modules/paging/PageMessage';
import { UrbanResultsPageEditor } from '../../modules/paging/editors/UrbanResultsPageEditor';
import { CommandMessageContext } from '../CommandMessageContext';

class UrbanDictionaryCommand extends Command {
    constructor() {
        super({
            name: 'urbandictionary',
            aliases: ['urban'],
            group: 'Knowledge ❓',
            description: 'Search on UrbanDictionary!',
            examples: ['ffs', 'tea'],
            maxDailyUseCount: 100,

            args: [
                {
                    key: 'term',
                    label: 'term',
                    type: 'text',
                    prompt: 'What term would you like to know the meaning of?'
                }
            ]
        });
    }

    async run({ message, client, author }: CommandMessageContext, { term }: { term: string }): Promise<void> {
        const { channel } = message;

        const results = await UrbanDictionaryModule.search(term);
        if (results.length === 0)
            throw new CommandError(`Could not find results on UrbanDictionary for '${term}'.`);

        await new PageMessage(
            client,
            author,
            results,
            new UrbanResultsPageEditor(DiscordEmbedFormatter.baseUserEmbed(author)),
            { cancellable: true }
        ).send(channel);
    }
}

export = UrbanDictionaryCommand;
