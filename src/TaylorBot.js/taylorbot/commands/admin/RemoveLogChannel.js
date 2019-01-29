'use strict';

const UserGroups = require('../../client/UserGroups.js');
const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class RemoveLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'removelogchannel',
            aliases: ['rlc'],
            group: 'admin',
            description: 'Indicates the bot that it should stop logging in a channel.',
            minimumGroup: UserGroups.GuildOwners,
            examples: ['#joinlogs', 'log'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to stop logging in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { channel }) {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (!textChannel.is_log) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is not a log channel.`);
        }

        await database.textChannels.removeLog(channel);
        return client.sendEmbedSuccess(message.channel, `Successfully removed ${Format.guildChannel(channel, '#name (`#id`)')} as a log channel.`);
    }
}

module.exports = RemoveLogChannelCommand;