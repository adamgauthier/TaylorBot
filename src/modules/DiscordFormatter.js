'use strict';

class DiscordFormatter {
    static user(user, formatString = '#name (#id)') {
        return formatString
            .replace('#name', user.username)
            .replace('#id', user.id);
    }

    static guild(guild, formatString = '#name (#id)') {
        return formatString
            .replace('#name', guild.name)
            .replace('#id', guild.id);
    }

    static member(guildMember, formatString = '#name (#id), #gName (#gId)') {
        return formatString
            .replace('#username', guildMember.user.username)
            .replace('#nickname', guildMember.nickname)
            .replace('#name', guildMember.displayName)
            .replace('#id', guildMember.id)
            .replace('#gName', guildMember.guild.name)
            .replace('#gId', guildMember.guild.id);
    }

    static guildChannel(guildChannel, formatString = '#name (#id)') {
        return formatString
            .replace('#name', guildChannel.name)
            .replace('#id', guildChannel.id)
            .replace('#gName', guildChannel.guild.name)
            .replace('#gId', guildChannel.guild.id);
    }
}

module.exports = DiscordFormatter;