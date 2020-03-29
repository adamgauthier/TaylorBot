import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { SimpleStatPresenter } from '../member-presenters/SimpleStatPresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { Guild, GuildMember } from 'discord.js';

class MessagesAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'messages',
            aliases: [],
            description: 'total messages sent',
            columnName: 'message_count',
            singularName: 'message',
            presenter: SimpleStatPresenter
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
