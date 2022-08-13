import moment = require('moment');

import { Command } from '../../commands/Command';
import { CommandError } from '../../commands/CommandError';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { CommandMessageContext } from '../CommandMessageContext';
import BirthdayUserAttribute = require('../../attributes/user/Birthday');
import { ChannelType } from 'discord.js';

class SetPrivateBirthdayCommand extends Command {
    constructor() {
        super({
            name: 'setprivatebirthday',
            aliases: ['setprivatebd'],
            group: 'attributes',
            description: 'Sets your birthday privately. You will still get points on your birthday, horoscope and age commands will work.',
            examples: ['1989-12-13'],

            args: [
                {
                    key: 'value',
                    label: 'date',
                    type: 'birthday',
                    prompt: 'What do you want to set your birth date to?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { value }: { value: moment.Moment }): Promise<void> {
        const { client, message, author } = commandContext;
        if (message.channel.type !== ChannelType.DM) {
            throw new CommandError(`This command is meant to be used in DMs to protect your privacy. Type \`setprivatebirthday\` in a direct message.`);
        }

        const { database, registry } = client.master;

        const attribute = registry.attributes.get('birthday') as BirthdayUserAttribute;
        const { birthday } = await attribute.setBirthday(database, author, value, true);

        const parsed = moment.utc(birthday, 'YYYY-MM-DD');

        await client.sendEmbed(
            message.channel,
            DiscordEmbedFormatter
                .baseUserSuccessEmbed(author)
                .setDescription([
                    `Your birthday has been privately set to ${parsed.format('MMMM Do')}.`,
                    `You will still receive birthday points and can still use the \`horoscope\` and \`age\` commands.`
                ].join('\n'))
        );
    }
}

export = SetPrivateBirthdayCommand;
