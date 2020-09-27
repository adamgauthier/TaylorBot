import { Command } from '../Command';
import YoutubeModule = require('../../modules/google/YoutubeModule.js');
import { CommandError } from '../CommandError';
import PageMessage = require('../../modules/paging/PageMessage.js');
import TextPageEditor = require('../../modules/paging/editors/TextPageEditor.js');
import { CommandMessageContext } from '../CommandMessageContext';

class YoutubeCommand extends Command {
    constructor() {
        super({
            name: 'youtube',
            aliases: ['yt', 'ytn', 'ytt'],
            group: 'Media 📷',
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

    async run({ message, client }: CommandMessageContext, { search }: { search: string }): Promise<void> {
        const { author, channel } = message;

        const results: any[] = await YoutubeModule.searchVideo(search);

        if (results.length === 0)
            throw new CommandError(`No YouTube videos found for search '${search}'.`);

        await new PageMessage(
            client,
            author,
            results.map(r => `https://youtu.be/${r.id.videoId}`),
            new TextPageEditor(),
            { cancellable: true }
        ).send(channel);
    }
}

export = YoutubeCommand;
