import { Command } from '../Command';
import { YoutubeModule } from '../../modules/google/YoutubeModule';
import { CommandError } from '../CommandError';
import { PageMessage } from '../../modules/paging/PageMessage';
import { TextPageEditor } from '../../modules/paging/editors/TextPageEditor';
import { CommandMessageContext } from '../CommandMessageContext';

class YoutubeCommand extends Command {
    constructor() {
        super({
            name: 'youtube',
            aliases: ['yt', 'ytn', 'ytt'],
            group: 'Media 📷',
            description: 'Searches YouTube for videos.',
            examples: ['taylor swift begin again', 'brockhampton gold'],
            maxDailyUseCount: 3,

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

    async run({ message, client, author }: CommandMessageContext, { search }: { search: string }): Promise<void> {
        const { channel } = message;

        const results = await YoutubeModule.searchVideo(search);

        if (results.length === 0)
            throw new CommandError(`No YouTube videos found for search '${search}'.`);

        await new PageMessage(
            client,
            author,
            results.map(r => `Use **/youtube** for a better command experience and higher daily limit.\nhttps://youtu.be/${r.id.videoId}`),
            new TextPageEditor(),
            { cancellable: true }
        ).send(channel);
    }
}

export = YoutubeCommand;
