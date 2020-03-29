import { NoisyInhibitor } from '../NoisyInhibitor';
import { Message } from 'discord.js';
import { TaylorBotClient } from '../../client/TaylorBotClient';
import { CachedCommand } from '../../client/registry/CachedCommand';

class DisabledChannelCommandInhibitor extends NoisyInhibitor {
    async getBlockedMessage({ client, message, prefix }: { message: Message; client: TaylorBotClient; prefix: string }, command: CachedCommand): Promise<{ log: string; ui: string } | null> {
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
