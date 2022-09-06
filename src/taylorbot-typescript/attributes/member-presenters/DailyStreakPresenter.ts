import { EmbedBuilder } from 'discord.js';
import { MemberAttributePresenter } from '../MemberAttributePresenter';
import { EmbedUtil } from '../../modules/discord/EmbedUtil';

export class DailyStreakPresenter implements MemberAttributePresenter {
    present(): EmbedBuilder {
        return EmbedUtil.error('This command has been removed. Please use </daily streak:870731803739168859> instead.');
    }

    presentRankEntry(): string {
        throw new Error('Deprecated');
    }
}
