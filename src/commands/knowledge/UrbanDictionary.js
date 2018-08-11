'use strict';

const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');
const Urban = require('../../modules/urban/UrbanDictionaryModule.js');

class UrbanDictionaryCommand extends Command {
    constructor() {
        super({
            name: 'urbandictionary',
            aliases: ['urban'],
            group: 'knowledge',
            description: 'Search on UrbanDictionary!',
            examples: ['urbandictionary ffs', 'urban tea'],

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

        return client.sendEmbed(channel, Urban.getResultEmbed(author, results[0]));
    }
}

module.exports = UrbanDictionaryCommand;