import { DiscordEmbedFormatter } from '../../modules/discord/DiscordEmbedFormatter';
import { UserAttributePresenter } from '../UserAttributePresenter.js';
import { EmbedBuilder, User } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class AgePresenter implements UserAttributePresenter {
    present(commandContext: CommandMessageContext, user: User): Promise<EmbedBuilder> {
        const embed = DiscordEmbedFormatter.baseUserSuccessEmbed(user);

        return Promise.resolve(
            embed
                .setColor('#f04747')
                .setDescription('This command has been moved to </birthday age:1016938623880400907>. Please use it instead! 😊')
        );
    }
}
