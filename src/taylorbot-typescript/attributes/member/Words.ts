import { SimpleStatMemberAttribute } from '../SimpleStatMemberAttribute';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { Guild, GuildMember } from 'discord.js';
import { AboutPresenter } from '../member-presenters/AboutPresenter';

class WordsAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'words',
            aliases: [],
            description: 'total words sent',
            columnName: 'word_count',
            singularName: 'word',
            presenter: AboutPresenter
        });
    }

    retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        return database.guildMembers.getRankedWordsFor(member);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedWords(guild, entries);
    }
}

export = WordsAttribute;
