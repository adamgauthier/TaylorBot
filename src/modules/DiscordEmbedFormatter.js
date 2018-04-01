'use strict';

const { MessageEmbed } = require('discord.js');
const { GlobalPaths } = require('globalobjects');

const StringUtil = require(GlobalPaths.StringUtil);

class DiscordEmbedFormatter {
    static member(member) {
        const { user } = member;
        const { status } = user.presence;
        const avatarURL = user.avatarURL();

        const embed = new MessageEmbed({
            'url': avatarURL,
            'author': {
                'name': user.tag,
                'url': avatarURL
            }
        });

        embed.setColor(DiscordEmbedFormatter.getColor(status));
        embed.setThumbnail(avatarURL);
        embed.addField('ID', user.id, true);

        return embed;
    }

    static getColor(status) {
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