import { MessageEmbed, User } from 'discord.js';
import { CommandMessageContext } from '../commands/CommandMessageContext';

export interface UserAttributePresenter {
    present(commandContext: CommandMessageContext, user: User, attribute: Record<string, any> & { rank: string }): Promise<MessageEmbed>;
}
