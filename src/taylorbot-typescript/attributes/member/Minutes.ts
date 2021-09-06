import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { MinutesPresenter } from '../member-presenters/MinutesPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { Guild, GuildMember } from 'discord.js';

class MinutesAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'minutes',
            aliases: [],
            description: 'total minutes active',
            columnName: 'minute_count',
            singularName: 'minute',
            presenter: MinutesPresenter
        });
    }

    async retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        if (member.guild.memberCount < 10000)
            return database.guildMembers.getRankedMinutesFor(member);

        return await database.guildMembers.getMinutesFor(member);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedMinutes(guild, entries);
    }
}

export = MinutesAttribute;
