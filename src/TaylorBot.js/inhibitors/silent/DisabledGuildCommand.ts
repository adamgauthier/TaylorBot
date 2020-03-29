import { SilentInhibitor } from '../SilentInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { Message } from 'discord.js';
import { TaylorBotClient } from '../../client/TaylorBotClient';

class DisabledGuildCommandInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: { message: Message; client: TaylorBotClient }, command: CachedCommand): Promise<string | null> {
        const { guild } = message;

        if (!guild)
            return null;

        const isDisabled = await client.master.registry.commands.getIsGuildCommandDisabled(guild, command);

        if (isDisabled) {
            return 'The command is disabled in this guild.';
        }

        return null;
    }
}

export = DisabledGuildCommandInhibitor;
