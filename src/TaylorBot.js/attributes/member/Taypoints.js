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
        return database.guildMembers.getRankedTaypointsFor(member);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedTaypoints(guild, entries);
    }
}

module.exports = TaypointsMemberAttribute;