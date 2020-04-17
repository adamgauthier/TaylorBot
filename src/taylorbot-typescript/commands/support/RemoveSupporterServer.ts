import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';

class RemoveSupporterServerCommand extends Command {
    constructor() {
        super({
            name: 'removesupporterserver',
            aliases: ['rss'],
            group: 'support',
            description: 'Removes the current server from your supporter servers.',
            examples: [''],
            guildOnly: true,

            args: []
        });
    }

    async run({ message, client }: CommandMessageContext): Promise<void> {
        const { author, guild } = message;
        if (author == null || guild == null)
            throw new Error('This command must be ran with an author and a guild.');

        const { database } = client.master;

        const proUser = await database.pros.getUser(author);

        if (proUser === null || (proUser.expires_at !== null && proUser.expires_at < new Date())) {
            throw new CommandError('Only supporters can remove their supporter servers! Learn more about supporting with the `support` command.');
        }

        await database.pros.removeUserProGuild(author, guild);

        const { guild_exists } = await database.pros.proGuildExists(guild);

        let removedChannels: { channel_id: string }[] | null = null;

        if (!guild_exists) {
            removedChannels = await database.textChannels.removeAllLogsInGuild(guild);
        }

        await client.sendEmbedSuccess(message.channel, `Successfully removed '${guild.name}' from your supporter servers.${removedChannels != null ? ` Removed \`${removedChannels.length}\` log channels.` : ''}`);
    }
}

export = RemoveSupporterServerCommand;
