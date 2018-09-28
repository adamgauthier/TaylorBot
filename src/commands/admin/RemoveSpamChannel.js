'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class RemoveSpamChannelCommand extends Command {
    constructor() {
        super({
            name: 'removespamchannel',
            aliases: ['rsc'],
            group: 'admin',
            description: 'Indicates the bot that it should stop considering a channel as spam.',
            minimumGroup: UserGroups.GuildOwners,
            examples: ['#spam', 'spam-and-bots'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to consider as non-spam?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { channel }) {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (!textChannel.is_spam) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is not a spam channel.`);
        }

        await database.textChannels.removeSpam(channel);
        return client.sendEmbedSuccess(message.channel, `Successfully removed ${Format.guildChannel(channel, '#name (`#id`)')} as a spam channel.`);
    }
}

module.exports = RemoveSpamChannelCommand;