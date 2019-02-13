'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const HeistStatsPresentor = require('../member-presentors/HeistStatsPresentor.js');

class HeistProfitsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistprofits',
            aliases: ['hprofits'],
            description: 'points gained through heists',
            columnName: 'heist_win_amount',
            singularName: 'won point',
            presentor: HeistStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', 'heist_win_count', [this.columnName, 'heist_lose_count', 'heist_lose_amount']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

module.exports = HeistProfitsMemberAttribute;