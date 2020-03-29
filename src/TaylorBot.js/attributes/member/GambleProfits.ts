import { GambleStatsPresenter } from '../member-presenters/GambleStatsPresenter';
import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class GambleProfitsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gambleprofits',
            aliases: ['gprofits'],
            description: 'points gained through gambling',
            columnName: 'gamble_win_amount',
            singularName: 'won point',
            presenter: GambleStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', 'gamble_win_count', [this.columnName, 'gamble_lose_count', 'gamble_lose_amount']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

export = GambleProfitsMemberAttribute;
