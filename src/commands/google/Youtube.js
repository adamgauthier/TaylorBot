'use strict';

const Command = require('../Command.js');
const YoutubeModule = require('../../modules/YoutubeModule.js');
const CommandError = require('../CommandError.js');
const ArrayTextPageMessage = require('../../modules/paging/ArrayTextPageMessage.js');

class YoutubeCommand extends Command {
    constructor() {
        super({
            name: 'youtube',
            aliases: ['yt', 'ytn', 'ytt'],
            group: 'google',
            description: 'Searches YouTube for videos.',
            examples: ['taylor swift begin again', 'brockhampton gold'],

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

        return new ArrayTextPageMessage(
            client,
            author,
            results.map(r => `https://youtu.be/${r.id.videoId}`)
        ).send(channel);
    }
}

module.exports = YoutubeCommand;