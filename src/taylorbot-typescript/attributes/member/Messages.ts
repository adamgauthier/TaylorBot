import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { Guild, GuildMember } from 'discord.js';
import { AboutPresenter } from '../member-presenters/AboutPresenter';

class MessagesAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'messages',
            aliases: [],
            description: 'total messages sent',
            columnName: 'message_count',
            singularName: 'message',
            presenter: AboutPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedMessagesFor(member);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedMessages(guild, entries);
    }
}

export = MessagesAttribute;
