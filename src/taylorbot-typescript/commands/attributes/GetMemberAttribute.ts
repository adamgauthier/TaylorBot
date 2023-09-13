import { GuildMember } from 'discord.js';
import { MemberAttribute } from '../../attributes/MemberAttribute';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class GetMemberAttributeCommand extends Command {
    constructor() {
        super({
            name: 'getmemberattribute',
            aliases: ['gma'],
            group: 'attributes',
            description: 'Gets a member attribute for a user in the current server.',
            examples: ['messages @Adam#0420', 'messages'],
            guildOnly: true,

            args: [
                {
                    key: 'attribute',
                    label: 'attribute',
                    type: 'member-attribute',
                    prompt: 'What attribute would you like to see from user?'
                },
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the attribute of?'
                }
            ]
        });
    }

    async run(commandContext: CommandMessageContext, { attribute, member }: { attribute: MemberAttribute; member: GuildMember }): Promise<void> {
        const { client, message } = commandContext;
        const embed = await attribute.getCommand(commandContext, member);
        if (embed === null) {
            return;
        }
        await client.sendEmbed(
            message.channel,
            embed
        );
    }
}

export = GetMemberAttributeCommand;
