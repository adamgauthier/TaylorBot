import { Channel, DMChannel, Guild, GuildChannel, GuildMember, Role, User } from 'discord.js';

export class Format {
    static user(user: User, formatString = '#name (#id)'): string {
        return formatString
            .replace('#name', user.username)
            .replace('#id', user.id);
    }

    static guild(guild: Guild, formatString = '#name (#id)'): string {
        return formatString
            .replace('#name', guild.name)
            .replace('#id', guild.id);
    }

    static member(guildMember: GuildMember, formatString = '#name (#id), #gName (#gId)'): string {
        return formatString
            .replace('#username', guildMember.user.username)
            .replace('#nickname', guildMember.nickname)
            .replace('#name', guildMember.displayName)
            .replace('#id', guildMember.id)
            .replace('#gName', guildMember.guild.name)
            .replace('#gId', guildMember.guild.id);
    }

    static channel(channel: Channel): string {
        if (channel.type === 'dm')
            return Format.dmChannel(channel as DMChannel);
        else
            return Format.guildChannel(channel as GuildChannel);
    }

    static dmChannel(channel: DMChannel, formatString = 'DM with [#rName (#rId)] (#id)'): string {
        return formatString
            .replace('#rName', channel.recipient.username)
            .replace('#rId', channel.recipient.id)
            .replace('#id', channel.id);
    }

    static guildChannel(guildChannel: GuildChannel, formatString = '#name (#id) on #gName (#gId)'): string {
        return formatString
            .replace('#name', guildChannel.name)
            .replace('#id', guildChannel.id)
            .replace('#gName', guildChannel.guild.name)
            .replace('#gId', guildChannel.guild.id);
    }

    static role(role: Role, formatString = '#name (#id)'): string {
        return formatString
            .replace('#name', role.name)
            .replace('#id', role.id)
            .replace('#gName', role.guild.name)
            .replace('#gId', role.guild.id);
    }

    static escapeDiscordMarkdown(string: string): string {
        return string.replace(/(_|\*|`|~|^> )/gm, '\\$1');
    }
}
