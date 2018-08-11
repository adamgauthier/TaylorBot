'use strict';

const Command = require('../../structures/Command.js');
const CleverBot = require('../../modules/cleverbot/CleverBotModule.js');

class CleverBotCommand extends Command {
    constructor() {
        super({
            name: 'cleverbot',
            aliases: ['clever'],
            group: 'fun',
            description: 'Chat with Cleverbot!',
            examples: ['cleverbot hi how are you?', 'clever are you human?'],

            args: [
                {
                    key: 'text',
                    label: 'text',
                    type: 'text',
                    prompt: 'What would you like to say to Cleverbot?'
                }
            ]
        });
    }

    async run({ message, client }, { text }) {
        const { author, channel } = message;
        const { database } = client.master;

        channel.startTyping();
        try {
            const session = await database.cleverbotSessions.get(author);
            if (!session) {
                await CleverBot.create(author.id);
                await database.cleverbotSessions.add(author);
            }

            const answer = await CleverBot.ask(author.id, text);
            return client.sendEmbedSuccess(channel, `<@${author.id}> ${answer}`);
        }
        finally {
            channel.stopTyping();
        }
    }
}

module.exports = CleverBotCommand;