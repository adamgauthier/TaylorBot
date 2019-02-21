'use strict';

const Command = require('../Command.js');
const YoutubeModule = require('../../modules/google/YoutubeModule.js');
const CommandError = require('../CommandError.js');
const PageMessage = require('../../modules/paging/PageMessage.js');
const TextPageEditor = require('../../modules/paging/editors/TextPageEditor.js');

class YoutubeCommand extends Command {
    constructor() {
        super({
            name: 'youtube',
            aliases: ['yt', 'ytn', 'ytt'],
            group: 'media',
            description: 'Searches YouTube for videos.',
            examples: ['taylor swift begin again', 'brockhampton gold'],
            maxDailyUseCount: 100,

            args: [
                {
                    key: 'search',
                    label: 'search',
                    type: 'text',
                    prompt: 'What search text would you like to get youtube results for?'
                }
            ]
        });
    }

    async run({ message, client }, { search }) {
        const { author, channel } = message;

        const results = await YoutubeModule.searchVideo(search);

        if (results.length === 0)
            throw new CommandError(`No YouTube videos found for search '${search}'.`);

        return new PageMessage(
            client,
            author,
            results.map(r => `https://youtu.be/${r.id.videoId}`),
            new TextPageEditor(),
            { cancellable: true }
        ).send(channel);
    }
}

module.exports = YoutubeCommand;