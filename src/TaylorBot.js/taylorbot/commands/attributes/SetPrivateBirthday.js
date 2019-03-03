'use strict';

const moment = require('moment');

const Command = require('../../commands/Command.js');
const CommandError = require('../../commands/CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

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

    async run(commandContext, { value }) {
        const { client, message } = commandContext;
        if (message.channel.type !== 'dm') {
            throw new CommandError(`This command is meant to be used in DMs to protect your privacy. Type \`setprivatebirthday\` in a direct message.`);
        }

        const { database, registry } = client.master;

        const attribute = registry.attributes.get('birthday');
        const { birthday } = await attribute.setBirthday(database, message.author, value, true);

        const parsed = moment.utc(birthday, 'YYYY-MM-DD');

        return client.sendEmbed(
            message.channel,
            DiscordEmbedFormatter
                .baseUserEmbed(message.author)
                .setDescription([
                    `Your birthday has been privately set to ${parsed.format('MMMM Do')}.`,
                    `You will still receive birthday points and can still use the \`horoscope\` and \`age\` commands.`
                ].join('\n'))
        );
    }
}

module.exports = SetPrivateBirthdayCommand;