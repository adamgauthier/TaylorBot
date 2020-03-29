import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { GambleStatsPresenter } from '../member-presenters/GambleStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class GambleWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblewins',
            aliases: ['gwins'],
            description: 'total number of gambles won',
            columnName: 'gamble_win_count',
            singularName: 'won gamble',
            presenter: GambleStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', this.columnName, ['gamble_lose_count', 'gamble_win_amount', 'gamble_lose_amount']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

export = GambleWinsMemberAttribute;
