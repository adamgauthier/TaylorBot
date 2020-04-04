import { NoisyInhibitor } from '../NoisyInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class DisabledChannelCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ client, message, prefix }: MessageContext, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
        const { channel } = message;

        if (channel.type === 'text') {
            const isCommandDisabledInChannel = await client.master.registry.channelCommands.isCommandDisabledInChannel(channel, command);

            if (isCommandDisabledInChannel) {
                return {
                    ui: [
                        `You can't use \`${command.name}\` because it is disabled in ${channel}.`,
                        `You can re-enable it by typing \`${prefix}ecc ${command.name}\`.`
                    ].join('\n'),
                    log: 'The command is disabled in this channel.'
                };
            }
        }

        return null;
    }
}

export = DisabledChannelCommandInhibitor;
