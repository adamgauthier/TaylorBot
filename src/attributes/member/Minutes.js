'use strict';

const moment = require('moment');

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class MinutesAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'minutes',
            description: 'total minutes active'
        });
    }

    async getCommand({ client }, member) {
        const { minute_count, rank } = await client.master.database.guildMembers.getRankedMinutesFor(member);

        const duration = moment.duration(minute_count, 'minutes');

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has been active for ${StringUtil.plural(minute_count, 'minute', '**')} in this server.`,
                `This is roughly equivalent to **${duration.humanize()}** of activity.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedMinutes(guild, entries);
    }

    presentRankEntry(member, { rank, minute_count }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(minute_count, 'minute', '`')}`;
    }
}

module.exports = MinutesAttribute;