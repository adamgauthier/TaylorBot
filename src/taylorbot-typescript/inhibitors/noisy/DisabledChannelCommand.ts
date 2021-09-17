import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';
import UserGroups = require('../../client/UserGroups');

class DisabledChannelCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ client, message }: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        if (command.command.minimumGroup === UserGroups.Master)
            return null;

        const { channel } = message;

        if (channel.type === 'GUILD_TEXT' || channel.type === 'GUILD_NEWS' || channel.isThread()) {
            const isCommandDisabledInChannel = await client.master.registry.channelCommands.isCommandDisabledInChannel(channel, command);

            if (isCommandDisabledInChannel) {
                return {
                    ui: [
                        `You can't use \`${command.name}\` because it is disabled in ${channel}.`,
                        `You can re-enable it by typing **/command channel-enable ${command.name}**.`
                    ].join('\n'),
                    log: 'The command is disabled in this channel.'
                };
            }
        }

        return null;
    }
}

export = DisabledChannelCommandInhibitor;
