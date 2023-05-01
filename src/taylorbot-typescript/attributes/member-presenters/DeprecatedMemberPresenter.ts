import { EmbedBuilder, GuildMember } from 'discord.js';
import { MemberAttributePresenter } from '../MemberAttributePresenter';
import { EmbedUtil } from '../../modules/discord/EmbedUtil';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

export class DeprecatedMemberPresenter implements MemberAttributePresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, { newCommand }: Record<string, any> & { rank: string }): EmbedBuilder {
        return EmbedUtil.error(`This command has been removed. Please use ${newCommand} instead.`);
    }

    presentRankEntry(): string {
        throw new Error('Deprecated');
    }
}
