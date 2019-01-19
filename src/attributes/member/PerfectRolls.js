'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class PerfectRollsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'perfectrolls',
            aliases: ['prolls', '1989rolls'],
            description: 'total number of perfect rolls',
            columnName: 'perfect_roll_count',
            singularName: 'perfect roll'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'roll_stats', this.columnName);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'roll_stats', this.columnName);
    }
}

module.exports = PerfectRollsMemberAttribute;