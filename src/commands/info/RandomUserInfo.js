'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../../structures/Command.js');

class RandomUserInfoCommand extends Command {
    constructor() {
        super({
            name: 'randomuserinfo',
            aliases: ['randomuser', 'randomuinfo', 'ruinfo', 'rui'],
            group: 'info',
            description: 'Gets information about a random user in the server.',
            examples: ['randomuserinfo', 'rui'],
            guildOnly: true,

            args: []
        });
    }

    run({ message, client }) {
        const member = message.guild.members.random();

        return client.sendEmbed(message.channel, DiscordEmbedFormatter.member(member));
    }
}

module.exports = RandomUserInfoCommand;