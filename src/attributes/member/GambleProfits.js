'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const GambleStatsPresentor = require('../member-presentors/GambleStatsPresentor.js');

class GambleProfitsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gambleprofits',
            description: 'points gained through gambling',
            columnName: 'gamble_win_amount',
            singularName: 'won point',
            presentor: GambleStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'gamble_stats', this.columnName, ['gamble_lose_count', 'gamble_win_amount', 'gamble_lose_amount']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'gamble_stats', this.columnName);
    }
}

module.exports = GambleProfitsMemberAttribute;