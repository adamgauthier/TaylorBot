import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { HeistStatsPresenter } from '../member-presenters/HeistStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class HeistWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistwins',
            aliases: ['hwins'],
            description: 'total number of heists won',
            columnName: 'heist_win_count',
            singularName: 'won heist',
            presenter: HeistStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', this.columnName, ['heist_lose_count', 'heist_win_amount', 'heist_lose_amount']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

export = HeistWinsMemberAttribute;
