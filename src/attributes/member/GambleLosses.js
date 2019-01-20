'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const GambleStatsPresentor = require('../member-presentors/GambleStatsPresentor.js');

class GambleLossesMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblelosses',
            description: 'points lost through gambling',
            columnName: 'gamble_lose_amount',
            singularName: 'lost point',
            presentor: GambleStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', 'gamble_win_count', [this.columnName, 'gamble_win_amount', 'gamble_lose_amount']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

module.exports = GambleLossesMemberAttribute;