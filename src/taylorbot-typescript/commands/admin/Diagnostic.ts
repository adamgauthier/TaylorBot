import UserGroups = require('../../client/UserGroups');
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { Command } from '../Command';
import { CommandMessageContext } from '../CommandMessageContext';

class DiagnosticCommand extends Command {
    constructor() {
        super({
            name: 'diagnostic',
            group: 'admin',
            description: 'Gets diagnostic information a TaylorBot component.',
            minimumGroup: UserGroups.Master,
            examples: [''],

            args: [
                {
                    key: 'component',
                    label: 'component-name',
                    type: 'any-text',
                    prompt: 'What component would you like to see the diagnostic of?'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { component }: { component: string }): Promise<void> {
        // Command is going to be handled by another component
        if (component !== 'typescript')
            return;

        const embed = DiscordEmbedFormatter
            .baseUserSuccessEmbed(client.user!)
            .addFields([
                { name: 'Guild Cache', value: `${client.guilds.cache.size}`, inline: true },
                { name: 'User Cache', value: `${client.users.cache.size}`, inline: true },
                { name: 'Channel Cache', value: `${client.channels.cache.size}`, inline: true },
                { name: 'Uptime', value: `\`${client.uptime}\` ms`, inline: true },
            ]);

        if (client.shard != null)
            embed.addFields({ name: 'Shard Count', value: `${client.shard.count}`, inline: true });

        await client.sendEmbed(message.channel, embed);
    }
}

export = DiagnosticCommand;
