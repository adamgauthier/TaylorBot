'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const MinutesPresentor = require('../member-presentors/MinutesPresentor.js');

class MinutesAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'minutes',
            description: 'total minutes active',
            columnName: 'minute_count',
            singularName: 'minute',
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
