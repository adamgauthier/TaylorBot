'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);
const GoogleImagesModule = require('../../modules/GoogleImagesModule');
const CommandError = require(Paths.CommandError);

class ImageCommand extends Command {
    constructor() {
        super({
            name: 'image',
            aliases: ['imagen', 'imaget'],
            group: 'info',
            description: 'Searches images based on the search text provided.',
            examples: ['image taylor swift', 'image kanye west'],
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
        const { items, searchInformation } = await GoogleImagesModule.search(search);

        if (search.totalResults === '0')
            throw new CommandError(`No results found for search '${search}'.`);

        const item = items[0];

        const embed = DiscordEmbedFormatter
            .baseUserHeader(message.author)
            .setTitle(item.title)
            .setURL(item.image.contextLink)
            .setImage(item.link)
            .setFooter(`${searchInformation.formattedTotalResults} results found in ${searchInformation.formattedSearchTime} seconds`);
        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = ImageCommand;