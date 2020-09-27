import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { Command } from '../Command';
import { version } from '../../package.json';
import { MASTER_ID } from '../../config/config.json';
import { CommandMessageContext } from '../CommandMessageContext';

class BotInfoCommand extends Command {
    constructor() {
        super({
            name: 'botinfo',
            aliases: ['binfo', 'clientinfo', 'version'],
            group: 'Stats 📊',
            description: 'Gets general info about the bot.',
            examples: [''],

            args: []
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        const { user } = client;
        const embed = DiscordEmbedFormatter
            .baseUserEmbed(user)
            .addField('Version', `\`${version}\``, true)
            .addField('Uptime', `\`${client.uptime}\` ms`, true)
            .addField('Guild Store', client.guilds.size, true)
            .addField('User Store', client.users.size, true)
            .addField('Channel Store', client.channels.size, true)
            .addField('Author', `<@${MASTER_ID}>`, true)
            .addField('Language', 'typescript/javascript + C#', true)
            .addField('Library', 'discord.js + Discord.Net', true);

        await client.sendEmbed(message.channel, embed);
    }
}

export = BotInfoCommand;
