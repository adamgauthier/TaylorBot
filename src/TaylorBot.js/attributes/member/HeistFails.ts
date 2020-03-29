import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { HeistStatsPresenter } from '../member-presenters/HeistStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class HeistFailsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistfails',
            aliases: ['hfails'],
            description: 'total number of heists lost',
            columnName: 'heist_lose_count',
            singularName: 'lost heist',
            presenter: HeistStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', 'heist_win_count', [this.columnName, 'heist_win_amount', 'heist_lose_amount']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

export = HeistFailsMemberAttribute;
