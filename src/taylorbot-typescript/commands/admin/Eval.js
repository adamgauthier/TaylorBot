'use strict';

const EmbedUtil = require('../../modules/EmbedUtil.js');
const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const UserGroups = require('../../client/UserGroups.js');
const Log = require('../../tools/Logger.js');

class EvalCommand extends Command {
    constructor() {
        super({
            name: 'eval',
            group: 'admin',
            description: 'Evaluates code.',
            minimumGroup: UserGroups.Master,
            examples: ['1+1'],

            args: [
                {
                    key: 'code',
                    label: 'code',
                    type: 'multiline-text',
                    prompt: 'What code would you like to be evaluated?'
                }
            ]
        });
    }

    async run(commandMessage, { code }) {
        const { message, client } = commandMessage;

        let result;

        const start = process.hrtime.bigint();
        try {
            result = eval(code);
        } catch (e) {
            Log.error(`Eval error: ${e.stack}`);
            throw new CommandError(e.toString());
        }
        const end = process.hrtime.bigint();

        return client.sendEmbed(message.channel,
            EmbedUtil
                .success(result)
                .setFooter(`Evaluation took ${end - start} nanoseconds`)
        );
    }
}

module.exports = EvalCommand;
