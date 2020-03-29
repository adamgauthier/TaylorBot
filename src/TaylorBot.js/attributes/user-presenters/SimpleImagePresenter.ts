import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');
import { User, MessageEmbed } from 'discord.js';
import { UserAttribute } from '../UserAttribute.js';

export class SimpleImagePresenter implements UserAttributePresenter {
    readonly #attribute: UserAttribute;
    constructor(attribute: UserAttribute) {
        this.#attribute = attribute;
    }

    present(commandContext: CommandMessageContext, user: User, attribute: Record<string, any> & { rank: string }): Promise<MessageEmbed> {
        return Promise.resolve(
            DiscordEmbedFormatter
                .baseUserEmbed(user)
                .setTitle(`${user.username}'s ${this.#attribute.description}`)
                .setImage(this.#attribute.formatValue(attribute))
        );
    }
}
