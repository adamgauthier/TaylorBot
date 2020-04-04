import Command = require('../../commands/Command');
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { MemberAttribute } from '../MemberAttribute';

export class RankMemberAttributeCommand extends Command {
    #attribute: MemberAttribute;

    constructor(attribute: MemberAttribute) {
        super({
            name: `rank${attribute.id}`,
            aliases: attribute.aliases.map((a: string) => `rank${a}`),
            group: 'attributes',
            description: `Gets the ranking of the ${attribute.description} of users in the current server.`,
            examples: [''],
            guildOnly: true,

            args: []
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext): Promise<void> {
        const { guild, channel } = commandContext.message;
        if (guild === null)
            throw new Error(`This command can only be used in a guild.`);

        const pageMessage = await this.#attribute.rankCommand(commandContext, guild);

        await pageMessage.send(channel);
    }
}
