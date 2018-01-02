'use strict';

class DiscordFormatter {
    static formatUser(user, formatString = '#name (#id)') {
        return formatString
            .replace('#name', user.username)
            .replace('#id', user.id);
    }

    static formatGuild(guild, formatString = '#name (#id)') {
        return formatString
            .replace('#name', guild.name)
            .replace('#id', guild.id);
    }

    static formatMember(guildMember, formatString = '#name (#id), #guildName (#guildId)') {
        return formatString
            .replace('#username', guildMember.user.username)
            .replace('#nickname', guildMember.nickname)
            .replace('#name', guildMember.displayName)
            .replace('#id', guildMember.id)
            .replace('#guildName', guildMember.guild.name)
            .replace('#guildId', guildMember.guild.id);
    }
}

module.exports = DiscordFormatter;