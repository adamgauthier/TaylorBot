'use strict';

const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');
const UserGroups = require('../../client/UserGroups.json');
const Log = require('../../tools/Logger.js');

class EvalCommand extends Command {
    constructor() {
        super({
            name: 'eval',
            group: 'util',
            description: 'Evaluates code.',
            minimumGroup: UserGroups.Master,
            examples: ['1+1'],

            args: [
                {
                    key: 'code',
                    label: 'code',
                    type: 'multiline-text',
                    prompt: 'What role would you like to be dropped?'
                }
            ]
        });
    }

    async run(commandMessage, { code }) {
        const { message, client } = commandMessage;

        let result;
        try {
            result = eval(code);
        } catch (e) {
            Log.error(`Eval error: ${e.stack}`);
            throw new CommandError(e.toString());
        }

        return client.sendEmbedSuccess(message.channel, result);
    }
}

module.exports = EvalCommand;