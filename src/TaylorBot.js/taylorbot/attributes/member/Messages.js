'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class MessagesAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'messages',
            description: 'total messages sent',
            columnName: 'message_count',
            singularName: 'message'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedMessagesFor(member);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedMessages(guild, entries);
    }
}

module.exports = MessagesAttribute;