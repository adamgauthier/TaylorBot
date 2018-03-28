'use strict';

const moment = require('moment');
const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const { masterId } = require(GlobalPaths.TaylorBotConfig);
const DefaultGroups = require(GlobalPaths.DefaultGroups);

class CommandsWatcher extends MessageWatcher {
    constructor() {
        super(async (taylorbot, message) => {
            const { author } = message;
            if (author.bot)
                return;

            const { channel } = message;
            if (channel.type === 'text') {
                const { guild, member, content } = message;
                const { guildSettings } = taylorbot;
                const { prefix } = guildSettings.get(guild.id);

                let text = content.trim();
                if (text.startsWith(prefix)) {
                    text = text.substring(prefix.length);
                    const args = text.split(' ');
                    const commandName = args.shift().toLowerCase();

                    const { commandSettings, groupSettings, userSettings } = taylorbot;
                    const command = commandSettings.getCommand(commandName);

                    if (!command)
                        return;

                    Log.verbose(`${Format.user(author)} attempting to use '${command.name}' with args '${args.join(';')}' in ${Format.guildChannel(channel)} on ${Format.guild(guild)}.`);

                    if (!command.enabled) {
                        Log.verbose(`Command '${command.name}' can't be used because it is disabled.`);
                        return;
                    }

                    if (command.disabledIn[guild.id]) {
                        Log.verbose(`Command '${command.name}' can't be used in ${Format.guild(guild)} because it is disabled.`);
                        return;
                    }

                    if (!CommandsWatcher.groupHasAccess(member, command.minimumGroup.accessLevel, guildSettings, groupSettings)) {
                        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they don't have the minimum group '${command.minimumGroup.name}'.`);
                        return;
                    }

                    // TODO: Command Groups

                    const commandTime = new Date().getTime();
                    const { lastCommand, lastAnswered, ignoreUntil } = userSettings.get(author.id);

                    if (commandTime < ignoreUntil) {
                        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they are ignored until ${moment(ignoreUntil, 'x').format('MMM Do YY, H:mm:ss Z')}.`);
                        return;
                    }

                    if (lastAnswered < lastCommand) {
                        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
                        return;
                    }

                    userSettings.updateLastCommand(author, commandTime);

                    try {
                        await command.handler({
                            message,
                            author,
                            channel,
                            guild,
                            args
                        });
                    }
                    catch (e) {
                        Log.error(`Command '${command.name}' Error: ${e}`);
                    }
                    finally {
                        const answeredTime = new Date().getTime();
                        userSettings.updateLastAnswered(author, answeredTime);
                    }
                }
            }
        });
    }

    static groupHasAccess(member, minimumGroupLevel, guildSettings, groupSettings) {
        let { accessLevel } = member.id === masterId ? DefaultGroups.Master : DefaultGroups.Everyone;
        if (accessLevel >= minimumGroupLevel)
            return true;

        const guildRoles = guildSettings.get(member.guild.id).roleGroups;
        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => g);

        for (const group of ownedGroups) {
            accessLevel = groupSettings.get(group);
            if (accessLevel >= minimumGroupLevel)
                return true;
        }

        return false;
    };
}

module.exports = new CommandsWatcher();