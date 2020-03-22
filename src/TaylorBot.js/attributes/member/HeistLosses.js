'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const HeistStatsPresentor = require('../member-presentors/HeistStatsPresentor.js');

class HeistLossesMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistlosses',
            aliases: ['hlosses'],
            description: 'points lost through heists',
            columnName: 'heist_lose_amount',
            singularName: 'lost point',
            presentor: HeistStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', 'heist_win_count', [this.columnName, 'heist_win_amount', 'heist_lose_count']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

module.exports = HeistLossesMemberAttribute;