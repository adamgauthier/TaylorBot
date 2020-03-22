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

    async retrieve(database, member) {
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

    rank(database, guild, entries) {
        return database.guildMembers.getRankedFirstJoinedAt(guild, entries);
    }
}

module.exports = JoinedAttribute;