'use strict';

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

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

    async run({ message, client }) {
        const { author, guild } = message;
        const { database } = client.master;

        const proUser = await database.pros.getUser(author);

        if (proUser === null || (proUser.expires_at !== null && proUser.expires_at < new Date())) {
            throw new CommandError('Only supporters can remove their supporter servers! Learn more about supporting with the `support` command.');
        }

        await database.pros.removeUserProGuild(author, guild);

        return client.sendEmbedSuccess(message.channel, `Successfully removed '${guild.name}' from your supporter servers.`);
    }
}

module.exports = RemoveSupporterServerCommand;