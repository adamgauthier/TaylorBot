'use strict';

const moment = require('moment');
const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const { masterId } = require(GlobalPaths.TaylorBotConfig);
const UserGroups = require(GlobalPaths.UserGroups);

class CommandsWatcher extends MessageWatcher {
    constructor() {
        super(false);
    }

    async messageHandler(taylorbot, message) {
        const { author } = message;
        if (author.bot)
            return;

        const { channel } = message;
        if (channel.type === 'text') {
            const { guild, member, content } = message;
            const { oldRegistry } = taylorbot;
            const { prefix } = oldRegistry.guilds.get(guild.id);

            let text = content.trim();
            if (text.startsWith(prefix)) {
                text = text.substring(prefix.length);
                const args = text.split(' ');
                const commandName = args.shift().toLowerCase();
                const command = oldRegistry.commands.getCommand(commandName);

                if (!command)
                    return;

                Log.verbose(`${Format.user(author)} attempting to use '${command.name}' with args '${args.join(';')}' in ${Format.guildChannel(channel)} on ${Format.guild(guild)}.`);

                if (!command.enabled) {
                    Log.verbose(`Command '${command.name}' can't be used because it is disabled.`);
                    return;
                }
            }
        }
    }
}

module.exports = CommandsWatcher;