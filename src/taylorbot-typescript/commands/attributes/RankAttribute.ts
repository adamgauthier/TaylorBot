import { MemberAttribute } from '../../attributes/MemberAttribute';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class RankAttributeCommand extends Command {
    constructor() {
        super({
            name: 'rank',
            group: 'attributes',
            description: 'Gets the ranking of an attribute for the current server.',
            examples: ['messages'],
            guildOnly: true,

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    prompt: 'What would you like to see the ranking of?',
                    type: 'member-attribute'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { attribute }: { attribute: MemberAttribute }): Promise<void> {
        const { guild, channel } = commandContext.message;

        const pageMessage = await attribute.rankCommand(commandContext, guild!);

        await pageMessage.send(channel);
    }
}

export = RankAttributeCommand;
