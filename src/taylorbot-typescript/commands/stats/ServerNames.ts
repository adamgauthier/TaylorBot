import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { Command } from '../Command';
import { TimeUtil } from '../../modules/util/TimeUtil';
import { ArrayUtil } from '../../modules/util/ArrayUtil';
import { PageMessage } from '../../modules/paging/PageMessage';
import { EmbedDescriptionPageEditor } from '../../modules/paging/editors/EmbedDescriptionPageEditor';
import { CommandMessageContext } from '../CommandMessageContext';
import { Guild } from 'discord.js';

class ServerNamesCommand extends Command {
    constructor() {
        super({
            name: 'servernames',
            aliases: ['snames', 'guildnames', 'gnames'],
            group: 'Stats 📊',
            description: 'Gets a list of previous names for a server.',
            examples: [''],

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the names of?',
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message, client, author }: CommandMessageContext, { guild }: { guild: Guild }): Promise<void> {
        const { channel } = message;
        const guildNames = await client.master.database.guildNames.getHistory(guild, 75);
        const embed = DiscordEmbedFormatter.baseGuildHeader(guild);

        const lines = guildNames.map(gn => `${TimeUtil.formatSmall(gn.changed_at.getTime())} : ${gn.guild_name}`);
        const chunks = ArrayUtil.chunk(lines, 15);

        await new PageMessage(
            client,
            author,
            chunks.map(chunk => chunk.join('\n')),
            new EmbedDescriptionPageEditor(embed)
        ).send(channel);
    }
}

export = ServerNamesCommand;
