'use strict';

const { MessageEmbed } = require('discord.js');

class EmbedUtil {
    static success(text) {
        return new MessageEmbed()
            .setColor('#43b581')
            .setDescription(text);
    }

    static error(text) {
        return new MessageEmbed()
            .setColor('#f04747')
            .setDescription(text);
    }
}

module.exports = EmbedUtil;