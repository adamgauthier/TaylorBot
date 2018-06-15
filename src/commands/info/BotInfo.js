'use strict';

const { Paths } = require('globalobjects');

const DiscordEmbedFormatter = require(Paths.DiscordEmbedFormatter);
const Command = require(Paths.Command);

const { version } = require('../../package.json');
const { MASTER_ID } = require('../../config/config.json');

class BotInfoCommand extends Command {
    constructor() {
        super({
            name: 'botinfo',
            aliases: ['binfo', 'clientinfo', 'version'],
            group: 'info',
            description: 'Gets general info about the bot.',
            examples: ['botinfo', 'binfo'],

            args: []
        });
    }

    run({ message, client }) {
        const { user } = client;
        const embed = DiscordEmbedFormatter
            .baseUserHeader(user)
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