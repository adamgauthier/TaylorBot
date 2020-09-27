import { UserAttribute } from '../UserAttribute';
import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { UserAttributePresenter } from '../UserAttributePresenter';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import { User, MessageEmbed } from 'discord.js';

export class SimplePresenter implements UserAttributePresenter {
    readonly #attribute: UserAttribute;
    constructor(attribute: UserAttribute) {
        this.#attribute = attribute;
    }

    present(commandContext: CommandMessageContext, user: User, attribute: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        return Promise.resolve(
            DiscordEmbedFormatter
                .baseUserEmbed(user)
                .setTitle(`${user.username}'s ${this.#attribute.description}`)
                .setDescription(this.#attribute.formatValue(attribute))
        );
    }
}
