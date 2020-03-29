import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { SimpleStatPresenter } from '../member-presenters/SimpleStatPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class RpsWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'rpswins',
            aliases: [],
            description: 'rock paper scissors wins',
            columnName: 'rps_win_count',
            singularName: 'rock paper scissors win',
            presenter: SimpleStatPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'rps_stats', this.columnName);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'rps_stats', this.columnName);
    }
}

export = RpsWinsMemberAttribute;
