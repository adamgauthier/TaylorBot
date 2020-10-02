import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { User, MessageEmbed } from 'discord.js';
import { UserAttribute } from '../UserAttribute.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class SimpleImagePresenter implements UserAttributePresenter {
    readonly #attribute: UserAttribute;
    constructor(attribute: UserAttribute) {
        this.#attribute = attribute;
    }

    present(commandContext: CommandMessageContext, user: User, attribute: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        return Promise.resolve(
            DiscordEmbedFormatter
                .baseUserSuccessEmbed(user)
                .setTitle(`${user.username}'s ${this.#attribute.description}`)
                .setImage(this.#attribute.formatValue(attribute))
        );
    }
}
