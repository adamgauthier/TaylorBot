import { Command } from '../../commands/Command';
import { GuildMember } from 'discord.js';
import { MemberAttribute } from '../MemberAttribute.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class GetMemberAttributeCommand extends Command {
    readonly #attribute: MemberAttribute;

    constructor(attribute: MemberAttribute) {
        super({
            name: attribute.id,
            aliases: attribute.aliases,
            group: 'attributes',
            description: `Gets the ${attribute.description} of a user in the current server.`,
            examples: ['@Adam#0420'],
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

    async run(commandContext: CommandMessageContext, { member }: { member: GuildMember }): Promise<void> {
        const { client, message } = commandContext;
        await client.sendEmbed(
            message.channel,
            await this.#attribute.getCommand(commandContext, member)
        );
    }
}
