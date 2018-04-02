'use strict';

const { MessageEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

const TimeUtil = require(GlobalPaths.TimeUtil);
const StringUtil = require(GlobalPaths.StringUtil);

class DiscordEmbedFormatter {
    static member(member) {
        const { user, client } = member;
        const { presence } = user;
        const { status } = presence;

        const avatarURL = user.displayAvatarURL({
            format: user.avatar ? user.avatar.startsWith('a_') ? 'gif' : 'jpg' : undefined
        });

        const shared = client.guilds.filterArray(g =>
            g.members.exists('id', member.id)
        ).map(g => g.name);

        const roles = member.roles.map(r => r.name);

        const embed = new MessageEmbed()
            .setURL(avatarURL)
            .setAuthor(`${(user.bot ? '🤖' : '👤')} ${user.tag}`, undefined, avatarURL)
            .setColor(DiscordEmbedFormatter.getStatusColor(status))
            .setThumbnail(avatarURL)
            .addField('ID', `\`${user.id}\``, true);

        if (presence.activity)
            embed.addField('Activity', presence.activity.name, true);

        // TODO: Timezones
        embed
            .addField('Server Joined', TimeUtil.formatFull(member.joinedTimestamp))
            .addField('Account Created', TimeUtil.formatFull(user.createdTimestamp))
            .addField(`${roles.length} Role${roles.length > 1 ? 's' : ''}`, StringUtil.shrinkString(roles.join(', '), 75, ', ...', [',']))
            .addField(`Shares ${shared.length} Server${shared.length > 1 ? 's' : ''}`, StringUtil.shrinkString(shared.join(', '), 75, ', ...', [',']));

        return embed;
    }

    static getStatusColor(status) {
        switch (status) {
            case 'online':
                return '#43b581';
            case 'idle':
                return '#faa61a';
            case 'dnd':
                return '#f04747';
            case 'offline':
                return '#747f8d';
            default:
                return 'RANDOM';
        }
    }
}

module.exports = DiscordEmbedFormatter;