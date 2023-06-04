import { EmbedBuilder, User } from 'discord.js';
import { EmbedUtil } from '../../modules/discord/EmbedUtil';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { UserAttributePresenter } from '../UserAttributePresenter';

export class DeprecatedUserPresenter implements UserAttributePresenter {
    present(commandContext: CommandMessageContext, user: User, { newCommand }: Record<string, any> & { rank: string }): Promise<EmbedBuilder> {
        return Promise.resolve(
            EmbedUtil.error(`This command has been removed. Please use ${newCommand} instead.`)
        );
    }
}
