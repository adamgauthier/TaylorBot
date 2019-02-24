'use strict';

const moment = require('moment');

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const CommandMessageContext = require('../../commands/CommandMessageContext.js');

class AgePresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    present(commandContext, user, { age, birthday }) {
        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        if (!birthday) {
            const setCommand = commandContext.client.master.registry.commands.resolve(`setbirthday`);
            const context = new CommandMessageContext(commandContext.messageContext, setCommand);

            return embed
                .setColor('#f04747')
                .setDescription([
                    `${user.username} is **${age.integer_value}** years old.`,
                    `⚠ This user has set their age manually so it might be outdated. ⚠`,
                    `Please use \`${context.usage()}\`, your age will automatically update and you will get points on your birthday every year! 🎈`
                ].join('\n'));
        }

        const parsedBirthday = moment.utc(birthday.birthday, 'YYYY-MM-DD');

        if (parsedBirthday.year() === 1804) {
            const setCommand = commandContext.client.master.registry.commands.resolve(`setbirthday`);
            const context = new CommandMessageContext(commandContext.messageContext, setCommand);

            return embed
                .setColor('#f04747')
                .setDescription([
                    `I don't know how old ${user.username} is because their birthday was set without a year. 😕`,
                    `Please use \`${context.usage()}\`, your age will automatically update and you will get points on your birthday every year! 🎈`
                ].join('\n'));
        }

        const computedAge = moment.utc().diff(parsedBirthday, 'years');

        return embed.setDescription(`${user.username} is **${computedAge}** years old.`);
    }
}

module.exports = AgePresentor;