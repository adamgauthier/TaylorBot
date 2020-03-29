import Command = require('../../commands/Command.js');
import { GuildMember, Message } from 'discord.js';
import { MemberAttribute } from '../MemberAttribute.js';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');

export class GetMemberAttributeCommand extends Command {
    readonly #attribute: MemberAttribute;

    constructor(attribute: MemberAttribute) {
        super({
            name: attribute.id,
            aliases: attribute.aliases,
            group: 'attributes',
            description: `Gets the ${attribute.description} of a user in the current server.`,
            examples: ['@Enchanted13#1989'],
            guildOnly: true,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: `What user would you like to see the ${attribute.description} of?`
                }
            ]
        });
        this.#attribute = attribute;
    }

    async run(commandContext: CommandMessageContext, { member }: { member: GuildMember }): Promise<Message> {
        const { client, message } = commandContext;
        return client.sendEmbed(
            message.channel,
            await this.#attribute.getCommand(commandContext, member)
        );
    }
}
