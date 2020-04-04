import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { SimpleStatPresenter } from '../member-presenters/SimpleStatPresenter';
import { GuildMember, Guild } from 'discord.js';
import { DatabaseDriver } from '../../database/DatabaseDriver';

class TaypointsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'taypoints',
            aliases: ['points'],
            description: 'taypoints',
            columnName: 'taypoint_count',
            singularName: 'taypoint',
            presenter: SimpleStatPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedTaypointsFor(member);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedTaypoints(guild, entries);
    }
}

export = TaypointsMemberAttribute;
