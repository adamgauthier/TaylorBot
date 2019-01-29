'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const MinutesPresentor = require('../member-presentors/MinutesPresentor.js');

class MinutesAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'minutes',
            description: 'total minutes active',
            columnName: 'minute_count',
            presentor: MinutesPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedMinutesFor(member);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedMinutes(guild, entries);
    }
}

module.exports = MinutesAttribute;