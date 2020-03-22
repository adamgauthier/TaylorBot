'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DailyStreakPresentor = require('../member-presentors/DailyStreakPresentor.js');

class DailyPayoutStreakMemberAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'dailypayoutstreak',
            aliases: ['dailystreak', 'dstreak'],
            description: 'daily payout streak',
            columnName: 'streak_count',
            presentor: DailyStreakPresentor
        });
    }

    retrieve(database, member) {
        return database.guildMembers.getRankedForeignStatFor(member, 'users', 'daily_payouts', this.columnName);
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedForeignStat(guild, entries, 'users', 'daily_payouts', this.columnName);
    }
}

module.exports = DailyPayoutStreakMemberAttribute;