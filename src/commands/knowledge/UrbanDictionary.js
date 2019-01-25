'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const Urban = require('../../modules/urban/UrbanDictionaryModule.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const PageMessage = require('../../modules/paging/PageMessage.js');
const UrbanResultsPageEditor = require('../../modules/paging/editors/UrbanResultsPageEditor.js');

class UrbanDictionaryCommand extends Command {
    constructor() {
        super({
            name: 'urbandictionary',
            aliases: ['urban'],
            group: 'knowledge',
            description: 'Search on UrbanDictionary!',
            examples: ['ffs', 'tea'],

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

    async run({ message, client }, { term }) {
        const { author, channel } = message;

        const results = await Urban.search(term);
        if (results.length === 0)
            throw new CommandError(`Could not find results on UrbanDictionary for '${term}'.`);

        return new PageMessage(
            client,
            author,
            results,
            new UrbanResultsPageEditor(DiscordEmbedFormatter.baseUserEmbed(author)),
            { cancellable: true }
        ).send(channel);
    }
}

module.exports = UrbanDictionaryCommand;