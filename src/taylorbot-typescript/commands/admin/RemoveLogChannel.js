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
            description: 'Stop the bot from logging a type of logs in a channel.',
            minimumGroup: UserGroups.GuildManagers,
            examples: ['member #joinlogs', 'message #message-log'],
            guildOnly: true,

            args: [
                {
                    key: 'type',
                    label: 'log-type',
                    prompt: 'What type of log channel do you want to remove?',
                    type: 'channel-log-type'
                },
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to stop logging in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { type, channel }) {
        const { database } = client.master;
        const textChannel = await database.textChannels.get(channel);

        if (!textChannel) {
            await database.textChannels.insertChannel(channel);
        }
        else if (!textChannel[`is_${type}_log`]) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is not a ${type} log channel.`);
        }
        else {
            await database.textChannels.removeLog(channel, type);
            if (type === 'message')
                await client.master.registry.redisCommands.hashSet('message-log-channels', channel.guild.id, '');
        }

        return client.sendEmbedSuccess(message.channel, `Successfully removed ${Format.guildChannel(channel, '#name (`#id`)')} as a ${type} log channel.`);
    }
}

module.exports = RemoveLogChannelCommand;
