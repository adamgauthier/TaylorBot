import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { ImgurModule } from '../../modules/imgur/ImgurModule';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { CommandMessageContext } from '../CommandMessageContext';

class ImgurCommand extends Command {
    constructor() {
        super({
            name: 'imgur',
            group: 'Media 📷',
            description: `Upload a picture on Imgur! If it's not already uploaded to a website, you can add it as an attachment to your command.`,
            examples: ['https://www.example.com/link/to/picture.jpg'],
            maxDailyUseCount: 10,

            args: [
                {
                    key: 'url',
                    label: 'url',
                    type: 'url-or-attachment',
                    prompt: `What's the link to the picture you would you like upload?`
                }
            ]
        });
    }

    async run({ message, client, author }: CommandMessageContext, { url }: { url: URL }): Promise<void> {
        const { channel } = message;

        const response = await ImgurModule.upload(url);

        if (!response.success)
            throw new CommandError(`Something went wrong when uploading to Imgur. 😕`);

        const { link } = response.data;

        await client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserSuccessEmbed(author)
            .setDescription(`Successfully uploaded your image to Imgur, it can be found here: ${link} 😊.`)
            .setImage(link)
        );
    }
}

export = ImgurCommand;
