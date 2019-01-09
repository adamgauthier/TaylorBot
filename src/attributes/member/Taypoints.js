'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class TaypointsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'taypoints',
            aliases: ['points'],
            description: 'taypoints',
            columnName: 'taypoint_count',
            singularName: 'taypoint'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'users', this.columnName);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'users', this.columnName);
    }
}

module.exports = TaypointsMemberAttribute;