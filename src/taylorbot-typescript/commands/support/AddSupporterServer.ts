import Command = require('../Command.js');
import CommandError = require('../CommandError.js');
import { CommandMessageContext } from '../CommandMessageContext';

class AddSupporterServerCommand extends Command {
    constructor() {
        super({
            name: 'addsupporterserver',
            aliases: ['ass'],
            group: 'support',
            description: 'Adds the current server in your supporter servers.',
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
            throw new CommandError('Only supporters can add their supporter servers! Learn more about supporting with `support` command.');
        }

        const { count } = await database.pros.countUserProGuilds(author);

        if (Number.parseInt(count) + 1 > proUser.subscription_count) {
            throw new CommandError(`You already have set ${count} supporter servers! If you want to change them, you can use the \`removesupporterserver\` command to first remove your supporter servers.`);
        }

        await database.pros.addUserProGuild(author, guild);

        await client.sendEmbedSuccess(message.channel, `Successfully added '${guild.name}' to your supporter servers.`);
    }
}

export = AddSupporterServerCommand;
