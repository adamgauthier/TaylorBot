'use strict';

const SimpleStatMemberAttribute = require('../SimpleStatMemberAttribute.js');

class RpsWinsMemberAttribute extends SimpleStatMemberAttribute {
    constructor() {
        super({
            id: 'rpswins',
            description: 'rock paper scissors wins',
            columnName: 'rps_wins',
            singularName: 'rock paper scissors win'
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'rps_stats', this.columnName);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'rps_stats', this.columnName);
    }
}

module.exports = RpsWinsMemberAttribute;