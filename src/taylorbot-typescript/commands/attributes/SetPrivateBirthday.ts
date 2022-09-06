import { Command } from '../../commands/Command';
import { CommandMessageContext } from '../CommandMessageContext';

class SetPrivateBirthdayCommand extends Command {
    constructor() {
        super({
            name: 'setprivatebirthday',
            aliases: ['setprivatebd'],
            group: 'attributes',
            description: 'This command has been removed. Please use **/birthday set** with the **privately** option instead.',
            examples: [''],

            args: [
                {
                    key: 'args',
                    label: 'args',
                    type: 'any-text',
                    prompt: 'What arguments would you like to use?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        await client.sendEmbedError(
            message.channel,
            `This command has been removed. Please use **/birthday set** with the **privately** option instead.`
        );
    }
}

export = SetPrivateBirthdayCommand;
