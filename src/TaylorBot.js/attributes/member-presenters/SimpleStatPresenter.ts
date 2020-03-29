import DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
import StringUtil = require('../../modules/StringUtil.js');
import MathUtil = require('../../modules/MathUtil.js');
import DiscordFormatter = require('../../modules/DiscordFormatter.js');
import { MemberAttributePresenter } from '../MemberAttributePresenter.js';
import { GuildMember, MessageEmbed } from 'discord.js';
import CommandMessageContext = require('../../commands/CommandMessageContext.js');
import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute.js';
import { MemberAttribute } from '../MemberAttribute.js';

export class SimpleStatPresenter implements MemberAttributePresenter {
    protected attribute: SimpleStatMemberAttribute;

    constructor(attribute: MemberAttribute) {
        if (!(attribute instanceof SimpleStatMemberAttribute))
            throw new Error('Attribute must be simple stat.');
        this.attribute = attribute as SimpleStatMemberAttribute;
    }

    present(commandContext: CommandMessageContext, member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): MessageEmbed {
        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has ${StringUtil.plural(stat, this.attribute.singularName, '**')}.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    presentRankEntry(member: GuildMember, { [this.attribute.columnName]: stat, rank }: Record<string, any> & { rank: string }): string {
        return `${rank}: ${DiscordFormatter.escapeDiscordMarkdown(member.user.username)} - ${StringUtil.plural(stat, this.attribute.singularName, '`')}`;
    }
}
