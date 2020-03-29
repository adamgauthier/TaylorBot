import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute.js';
import { DatabaseDriver } from '../../database/DatabaseDriver.js';
import { GuildMember, Guild } from 'discord.js';
import { GambleStatsPresenter } from '../member-presenters/GambleStatsPresenter.js';

class GambleFailsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblefails',
            aliases: ['gfails'],
            description: 'total number of gambles lost',
            columnName: 'gamble_lose_count',
            singularName: 'lost gamble',
            presenter: GambleStatsPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', 'gamble_win_count', [this.columnName, 'gamble_win_amount', 'gamble_lose_amount']);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

export = GambleFailsMemberAttribute;
