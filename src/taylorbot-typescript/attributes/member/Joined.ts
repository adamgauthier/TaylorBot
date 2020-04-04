import { MemberAttribute } from '../MemberAttribute';
import { JoinedPresenter } from '../member-presenters/JoinedPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { GuildMember, Guild } from 'discord.js';

class JoinedAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'joined',
            aliases: [],
            description: 'first joined date',
            columnName: 'first_joined_at',
            presenter: JoinedPresenter
        });
    }

    async retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        const result = await database.guildMembers.getRankedFirstJoinedAtFor(member);

        if (result) {
            return result;
        }

        if (member.joinedTimestamp !== null) {
            await database.guildMembers.fixInvalidJoinDate(member);
            return await database.guildMembers.getRankedFirstJoinedAtFor(member);
        }

        return null;
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedFirstJoinedAt(guild, entries);
    }
}

export = JoinedAttribute;
