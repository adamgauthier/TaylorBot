'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const GambleStatsPresentor = require('../member-presentors/GambleStatsPresentor.js');

class GambleFailsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'gamblefails',
            description: 'total number of gambles lost',
            columnName: 'gamble_lose_count',
            singularName: 'lost gamble',
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

module.exports = GambleFailsMemberAttribute;