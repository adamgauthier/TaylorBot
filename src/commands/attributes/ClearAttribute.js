'use strict';

const Command = require('../../structures/Command.js');

class ClearAttributeCommand extends Command {
    constructor() {
        super({
            name: 'clear',
            aliases: ['clearattribute', 'ca'],
            group: 'attributes',
            description: 'Clears one of your attributes.',
            examples: ['clear bae'],

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'user-attribute',
                    prompt: 'What attribute do you want to clear?'
                }
            ]
        });
    }

    async run(commandContext, { attribute }) {
        const { client, message } = commandContext;

        return client.sendEmbed(
            message.channel,
            await attribute.clear(commandContext)
        );
    }
}

module.exports = ClearAttributeCommand;