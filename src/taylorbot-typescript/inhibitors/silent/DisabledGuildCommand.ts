import { SilentInhibitor } from '../SilentInhibitor';
import { CachedCommand } from '../../client/registry/CachedCommand';
import { MessageContext } from '../../structures/MessageContext';

class DisabledGuildCommandInhibitor extends SilentInhibitor {
    async shouldBeBlocked({ message, client }: MessageContext, command: CachedCommand): Promise<string | null> {
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
