import { MessageEmbed, GuildMember } from 'discord.js';
import CommandMessageContext = require('../commands/CommandMessageContext');

export interface MemberAttributePresenter {
    present(commandContext: CommandMessageContext, member: GuildMember, attribute: Record<string, any> & { rank: string }): MessageEmbed;

    presentRankEntry(member: GuildMember, attribute: Record<string, any> & { rank: string }): string;
}
