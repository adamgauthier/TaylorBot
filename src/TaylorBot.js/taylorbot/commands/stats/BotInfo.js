'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../Command.js');

const { version } = require('../../package.json');
const { MASTER_ID } = require('../../config/config.json');

class BotInfoCommand extends Command {
    constructor() {
        super({
            name: 'botinfo',
            aliases: ['binfo', 'clientinfo', 'version'],
            group: 'stats',
            description: 'Gets general info about the bot.',
            examples: [''],

            args: []
        });
    }

    run({ message, client }) {
        const { user } = client;
        const embed = DiscordEmbedFormatter
            .baseUserEmbed(user)
            .addField('Version', `\`${version}\``, true)
            .addField('Uptime', `\`${client.uptime}\` ms`, true)
            .addField('Voice Connections', client.voiceConnections.size, true)
            .addField('Guild Store', client.guilds.size, true)
            .addField('User Store', client.users.size, true)
            .addField('Channel Store', client.channels.size, true)
            .addField('Author', `<@${MASTER_ID}>`, true)
            .addField('Language', 'javascript', true)
            .addField('Library', 'discord.js', true);

        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = BotInfoCommand;