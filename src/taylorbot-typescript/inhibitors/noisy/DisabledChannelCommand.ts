import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';
import UserGroups = require('../../client/UserGroups');
import { ChannelType } from 'discord.js';

class DisabledChannelCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ client, message }: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        if (command.command.minimumGroup === UserGroups.Master)
            return null;

        const { channel } = message;

        if (channel.type === ChannelType.GuildText || channel.type === ChannelType.GuildNews || channel.isThread()) {
            const isCommandDisabledInChannel = await client.master.registry.channelCommands.isCommandDisabledInChannel(channel, command);

            if (isCommandDisabledInChannel) {
                return {
                    ui: [
                        `You can't use \`${command.name}\` because it is disabled in ${channel}.`,
                        `You can re-enable it by typing </command channel-disable:909694280703016991> ${command.name}.`
                    ].join('\n'),
                    log: 'The command is disabled in this channel.'
                };
            }
        }

        return null;
    }
}

export = DisabledChannelCommandInhibitor;
