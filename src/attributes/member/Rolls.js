'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class RollsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'rolls',
            description: 'total number of rolls',
            columnName: 'roll_count',
            singularName: 'roll'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'roll_stats', this.columnName);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'roll_stats', this.columnName);
    }
}

module.exports = RollsMemberAttribute;