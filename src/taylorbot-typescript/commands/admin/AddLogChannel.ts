import UserGroups = require('../../client/UserGroups');
import { Format } from '../../modules/discord/DiscordFormatter';
import { Command } from '../Command';
import { CommandError } from '../../commands/CommandError';
import { CommandMessageContext } from '../CommandMessageContext';
import { TextChannel } from 'discord.js';

class AddLogChannelCommand extends Command {
    constructor() {
        super({
            name: 'addlogchannel',
            aliases: ['alc'],
            group: 'admin',
            description: 'Set an existing channel as a type of log channel for the bot to log in.',
            minimumGroup: UserGroups.GuildManagers,
            examples: ['member #joinlogs', 'message #message-log'],
            guildOnly: true,

            args: [
                {
                    key: 'type',
                    label: 'log-type',
                    prompt: 'What type of log channel do you want to add? Member logs include joins, leaves and bans. Message logs include deleted messages.',
                    type: 'channel-log-type'
                },
                {
                    key: 'channel',
                    label: 'channel',
                    prompt: 'What channel would you like the bot to log in?',
                    type: 'guild-text-channel-or-current'
                }
            ]
        });
    }

    async run({ message, client }: CommandMessageContext, { type, channel }: { type: 'message' | 'member'; channel: TextChannel }): Promise<void> {
        const { database } = client.master;

        const { guild_exists } = await database.pros.proGuildExists(channel.guild);
        if (guild_exists === false) {
            throw new CommandError(`Log channels are restricted to supporter servers, use \`support\` for more info.`);
        }

        const logChannels = await database.textChannels.getAllLogChannelsInGuild(message.guild!, type);

        if (logChannels.some(c => c.channel_id === channel.id)) {
            throw new CommandError(`Channel ${Format.guildChannel(channel, '#name (`#id`)')} is already a ${type} log channel.`);
        }

        if (logChannels.length > 0) {
            throw new CommandError(`There can only be 1 ${type} log channel for a server.`);
        }

        await database.textChannels.upsertLogChannel(channel, type);
        if (type === 'message')
            await client.master.registry.redisCommands.hashSet('message-log-channels', channel.guild.id, channel.id);

        await client.sendEmbedSuccess(message.channel, `Successfully made ${Format.guildChannel(channel, '#name (`#id`)')} a ${type} log channel.`);
    }
}

export = AddLogChannelCommand;
