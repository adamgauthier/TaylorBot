'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const RollStatsPresentor = require('../member-presentors/RollStatsPresentor.js');

class RollsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'rolls',
            description: 'total number of rolls',
            columnName: 'roll_count',
            singularName: 'roll',
            presentor: RollStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'roll_stats', this.columnName, ['perfect_roll_count']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'roll_stats', this.columnName);
    }
}

module.exports = RollsMemberAttribute;