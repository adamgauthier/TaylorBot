import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { Command } from '../Command';
import GoogleImagesModule = require('../../modules/google/GoogleImagesModule.js');
import { CommandError } from '../CommandError';
import PageMessage = require('../../modules/paging/PageMessage.js');
import ImageResultsPageEditor = require('../../modules/paging/editors/ImageResultsPageEditor.js');
import { CommandMessageContext } from '../CommandMessageContext';

class ImageCommand extends Command {
    constructor() {
        super({
            name: 'image',
            aliases: ['imagen', 'imaget'],
            group: 'Media 📷',
            description: 'Searches images based on the search text provided.',
            examples: ['taylor swift', 'kanye west'],
            maxDailyUseCount: 10,
            guildOnly: true,
            proOnly: true,

            args: [
                {
                    key: 'search',
                    label: 'search',
                    type: 'text',
                    prompt: 'What search text would you like to get image results for?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { search }: { search: string }): Promise<void> {
        const { author, channel } = message;

        const {
            error, items, searchInformation
        } = await GoogleImagesModule.search(search, 10);

        if (error) {
            if (error.errors && error.errors[0].reason === 'dailyLimitExceeded')
                throw new CommandError(`Looks like our daily limit for Google Images searches (100) was exceeded. 😭`);
            else
                throw new CommandError(`Something went wrong while querying Google Images, sorry about that. 😕`);
        }

        if (searchInformation.totalResults === '0' || !items) {
            throw new CommandError(`No results found for search '${search}'.`);
        }

        const embed = DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setFooter(`${searchInformation.formattedTotalResults} results found in ${searchInformation.formattedSearchTime} seconds`);

        await new PageMessage(
            client,
            author,
            items,
            new ImageResultsPageEditor(embed),
            { cancellable: true }
        ).send(channel);
    }
}

export = ImageCommand;
