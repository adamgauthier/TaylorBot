import EmbedUtil = require('../../modules/EmbedUtil.js');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import UserGroups = require('../../client/UserGroups.js');
import Log = require('../../tools/Logger.js');
import { CommandMessageContext } from '../CommandMessageContext';

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

    async run(commandMessage: CommandMessageContext, { code }: { code: string }): Promise<void> {
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

        await client.sendEmbed(message.channel,
            EmbedUtil
                .success(result)
                .setFooter(`Evaluation took ${end - start} nanoseconds`)
        );
    }
}

export = EvalCommand;
