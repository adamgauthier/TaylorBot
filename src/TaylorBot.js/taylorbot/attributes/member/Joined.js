'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const JoinedPresentor = require('../member-presentors/JoinedPresentor.js');

class JoinedAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'joined',
            description: 'first joined date',
            columnName: 'first_joined_at',
            presentor: JoinedPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedFirstJoinedAtFor(member);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedFirstJoinedAt(guild, entries);
    }
}

module.exports = JoinedAttribute;