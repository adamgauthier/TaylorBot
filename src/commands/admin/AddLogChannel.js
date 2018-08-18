'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');

class AddLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'addlogchannel',
            aliases: ['alc'],
            group: 'admin',
            description: 'Indicates the bot that it should log in a channel.',
            minimumGroup: UserGroups.GuildOwners,
            examples: ['addlogchannel #joinlogs', 'alc log'],
            guildOnly: true,

            args: [
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to log in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { channel }) {
        const { database } = client.master;
        const logChannels = await database.textChannels.getAllLogChannelsInGuild(message.guild);

        if (logChannels.some(c => c.channel_id === channel.id)) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is already a log channel.`);
        }

        if (logChannels.length > 0) {
            throw new CommandError(`There can only be 1 log channel for a server.`);
        }

        await database.textChannels.setLogging(channel);
        return client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a log channel.`);
    }
}

module.exports = AddLogChannelCommand;