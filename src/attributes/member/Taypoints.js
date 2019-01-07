'use strict';

const MemberAttribute = require('../MemberAttribute.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const MathUtil = require('../../modules/MathUtil.js');
const StringUtil = require('../../modules/StringUtil.js');

class TaypointsMemberAttribute extends MemberAttribute {
    constructor() {
        super({
            id: 'taypoints',
            aliases: ['points'],
            description: 'taypoints'
        });
    }

    async getCommand({ client }, member) {
        const { taypoint_count, rank } = await client.master.database.guildMembers.getRankedTaypointsFor(member);

        return DiscordEmbedFormatter
            .baseUserEmbed(member.user)
            .setDescription([
                `${member.displayName} has ${StringUtil.plural(taypoint_count, 'taypoint', '**')}.`,
                `They are the **${MathUtil.formatNumberSuffix(rank)}** user of the server (excluding users who left).`
            ].join('\n'));
    }

    rank(database, guild, entries) {
        return database.guildMembers.getRankedTaypoints(guild, entries);
    }

    presentRankEntry(member, { rank, taypoint_count }) {
        return `${rank}: ${member.user.username} - ${StringUtil.plural(taypoint_count, 'taypoint', '`')}`;
    }
}

module.exports = TaypointsMemberAttribute;