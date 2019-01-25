'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');
const GoogleImagesModule = require('../../modules/google/GoogleImagesModule.js');
const CommandError = require('../CommandError.js');
const PageMessage = require('../../modules/paging/PageMessage.js');
const ImageResultsPageEditor = require('../../modules/paging/editors/ImageResultsPageEditor.js');

class ImageCommand extends Command {
    constructor() {
        super({
            name: 'image',
            aliases: ['imagen', 'imaget'],
            group: 'google',
            description: 'Searches images based on the search text provided.',
            examples: ['taylor swift', 'kanye west'],
            guildOnly: true,

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

    async run({ message, client }, { search }) {
        const { author, channel } = message;

        const {
            error, items, searchInformation, totalResults
        } = await GoogleImagesModule.search(search, 10);

        if (error) {
            if (error.errors && error.errors[0].reason === 'dailyLimitExceeded')
                throw new CommandError(`Looks like our daily limit for Google Images searches (100) was exceeded. 😭`);
            else
                throw new CommandError(`Something went wrong while querying Google Images, sorry about that. 😕`);
        }

        if (totalResults === '0')
            throw new CommandError(`No results found for search '${search}'.`);

        const embed = DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setFooter(`${searchInformation.formattedTotalResults} results found in ${searchInformation.formattedSearchTime} seconds`);

        return new PageMessage(
            client,
            author,
            items,
            new ImageResultsPageEditor(embed)
        ).send(channel);
    }
}

module.exports = ImageCommand;