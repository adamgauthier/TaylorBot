'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class WordsAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'words',
            description: 'total words sent',
            columnName: 'word_count',
            singularName: 'word'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedWordsFor(member);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedWords(guild, entries);
    }
}

module.exports = WordsAttribute;