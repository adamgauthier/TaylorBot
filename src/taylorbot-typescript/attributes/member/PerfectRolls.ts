import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { RollStatsPresenter } from '../member-presenters/RollStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class PerfectRollsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'perfectrolls',
            aliases: ['prolls', '1989rolls'],
            description: 'total number of perfect rolls',
            columnName: 'perfect_roll_count',
            singularName: 'perfect roll',
            presenter: RollStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'roll_stats', 'roll_count', [this.columnName]);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'roll_stats', this.columnName);
    }
}

export = PerfectRollsMemberAttribute;
