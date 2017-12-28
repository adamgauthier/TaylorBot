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
}

module.exports = DiscordFormatter;