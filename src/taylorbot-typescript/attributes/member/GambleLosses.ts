import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { GambleStatsPresenter } from '../member-presenters/GambleStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class GambleLossesMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblelosses',
            aliases: ['glosses'],
            description: 'points lost through gambling',
            columnName: 'gamble_lose_amount',
            singularName: 'lost point',
            presenter: GambleStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', 'gamble_win_count', [this.columnName, 'gamble_win_amount', 'gamble_lose_count']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

export = GambleLossesMemberAttribute;
