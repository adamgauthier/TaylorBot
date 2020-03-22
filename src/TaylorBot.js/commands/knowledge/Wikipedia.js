'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const Wikipedia = require('../../modules/wiki/WikipediaModule.js');

class WikipediaCommand extends Command {
    constructor() {
        super({
            name: 'wikipedia',
            aliases: ['wiki'],
            group: 'knowledge',
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

    async run({ message, client }, { title }) {
        const { author, channel } = message;

        const page = await Wikipedia.getPage(title);
        if (page.invalid)
            throw new CommandError(`Article title '${title}' is invalid.`);

        if (page.missing)
            throw new CommandError(`Could not find Wikipedia article for '${title}'.`);

        return client.sendEmbed(channel, Wikipedia.getPageEmbed(author, page));
    }
}

module.exports = WikipediaCommand;