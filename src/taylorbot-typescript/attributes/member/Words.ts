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

    async retrieve(database: DatabaseDriver, member: GuildMember): Promise<any> {
        if (member.guild.memberCount < 10000)
            return await database.guildMembers.getRankedWordsFor(member);

        return await database.guildMembers.getWordsFor(member);
    }

    rank(database: DatabaseDriver, guild: Guild, entries: number): Promise<any[]> {
        return database.guildMembers.getRankedWords(guild, entries);
    }
}

export = WordsAttribute;
