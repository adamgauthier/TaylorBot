'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const HeistStatsPresentor = require('../member-presentors/HeistStatsPresentor.js');

class HeistFailsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistfails',
            aliases: ['hfails'],
            description: 'total number of heists lost',
            columnName: 'heist_lose_count',
            singularName: 'lost heist',
            presentor: HeistStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', 'heist_win_count', [this.columnName, 'heist_win_amount', 'heist_lose_amount']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

module.exports = HeistFailsMemberAttribute;