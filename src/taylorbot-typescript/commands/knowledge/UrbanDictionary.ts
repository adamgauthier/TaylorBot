import { Command } from '../Command';
import { CommandError } from '../CommandError';
import Urban = require('../../modules/urban/UrbanDictionaryModule.js');
import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import PageMessage = require('../../modules/paging/PageMessage.js');
import UrbanResultsPageEditor = require('../../modules/paging/editors/UrbanResultsPageEditor.js');
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

    async run({ message, client }: CommandMessageContext, { term }: { term: string }): Promise<void> {
        const { author, channel } = message;

        const results = await Urban.search(term);
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
