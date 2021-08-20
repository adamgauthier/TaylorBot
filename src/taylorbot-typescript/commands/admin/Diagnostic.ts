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
            .addField('Guild Cache', `${client.guilds.cache.size}`, true)
            .addField('User Cache', `${client.users.cache.size}`, true)
            .addField('Channel Cache', `${client.channels.cache.size}`, true)
            .addField('Uptime', `\`${client.uptime}\` ms`, true);

        if (client.shard != null)
            embed.addField('Shard Count', `${client.shard.count}`, true);

        await client.sendEmbed(message.channel, embed);
    }
}

export = DiagnosticCommand;
