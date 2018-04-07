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

                    if (!CommandsWatcher.groupHasAccess(member, command.minimumGroup.accessLevel, oldRegistry.guilds, oldRegistry.groups)) {
                        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they don't have the minimum group '${command.minimumGroup.name}'.`);
                        return;
                    }
                }
            }
    }

    static groupHasAccess(member, minimumGroupLevel, guilds, groups) {
        let { accessLevel } = member.id === masterId ? UserGroups.Master : UserGroups.Everyone;
        if (accessLevel >= minimumGroupLevel)
            return true;

        const guildRoles = guilds.get(member.guild.id).roleGroups;
        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => g);

        for (const group of ownedGroups) {
            accessLevel = groups.get(group);
            if (accessLevel >= minimumGroupLevel)
                return true;
        }

        return false;
    };
}

module.exports = CommandsWatcher;