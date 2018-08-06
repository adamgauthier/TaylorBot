'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

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
        const textChannel = await database.textChannels.get(channel);

        if (textChannel.is_logging) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is already a log channel.`);
        }

        await database.textChannels.setLogging(channel);
        return client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a log channel.`);
    }
}

module.exports = AddLogChannelCommand;