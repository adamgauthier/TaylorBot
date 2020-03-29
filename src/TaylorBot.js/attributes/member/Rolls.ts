import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { RollStatsPresenter } from '../member-presenters/RollStatsPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class RollsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'rolls',
            aliases: [],
            description: 'total number of rolls',
            columnName: 'roll_count',
            singularName: 'roll',
            presenter: RollStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'roll_stats', this.columnName, ['perfect_roll_count']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'roll_stats', this.columnName);
    }
}

export = RollsMemberAttribute;
