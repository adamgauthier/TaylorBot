'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');
const HeistStatsPresentor = require('../member-presentors/HeistStatsPresentor.js');

class HeistWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'heistwins',
            aliases: ['hwins'],
            description: 'total number of heists won',
            columnName: 'heist_win_count',
            singularName: 'won heist',
            presentor: HeistStatsPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'heist_stats', this.columnName, ['heist_lose_count', 'heist_win_amount', 'heist_lose_amount']);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'heist_stats', this.columnName);
    }
}

module.exports = HeistWinsMemberAttribute;