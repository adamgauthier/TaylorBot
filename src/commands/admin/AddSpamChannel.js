'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Format = require('../../modules/DiscordFormatter');
const Command = require('../../structures/Command');
const CommandError = require('../../structures/CommandError');

class AddSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'addspamchannel',
            aliases: ['asc'],
            group: 'admin',
            description: 'Indicates the bot that it should consider a channel as spam.',
            minimumGroup: UserGroups.GuildOwners,
            examples: ['#spam', 'spam-and-bots'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to consider as spam?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { channel }) {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (textChannel.is_spam) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is already a spam channel.`);
        }

        await database.textChannels.setSpam(channel);
        return client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a spam channel.`);
    }
}

module.exports = AddSpamChannelCommand;