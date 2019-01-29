'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const CleverBot = require('../../modules/cleverbot/CleverBotModule.js');
const Log = require('../../tools/Logger.js');

class CleverBotCommand extends Command {
    constructor() {
        super({
            name: 'cleverbot',
            aliases: ['clever'],
            group: 'fun',
            description: 'Chat with Cleverbot!',
            examples: ['hi how are you?', 'are you human?'],

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

        let answer;

        channel.startTyping();
        try {
            const session = await database.cleverbotSessions.getRandom();
            if (!session) {
                await CleverBot.create(author.id);
                await database.cleverbotSessions.add(author);
            }

            answer = await CleverBot.ask(session.user_id, text);
        }
        catch (e) {
            Log.error(e.stack);
            throw new CommandError(`Something went wrong on the Cleverbot API side. Nothing we can do about it. 😔`);
        }
        finally {
            channel.stopTyping();
        }

        return client.sendEmbedSuccess(channel, `<@${author.id}> ${answer}`);
    }
}

module.exports = CleverBotCommand;