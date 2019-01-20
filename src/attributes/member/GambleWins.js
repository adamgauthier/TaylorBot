'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const GambleStatsPresentor = require('../member-presentors/GambleStatsPresentor.js');

class GambleWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblewins',
            description: 'total number of gambles won',
            columnName: 'gamble_win_count',
            singularName: 'won gamble',
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

module.exports = GambleWinsMemberAttribute;