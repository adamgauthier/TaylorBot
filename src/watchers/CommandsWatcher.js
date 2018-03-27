'use strict';

const moment = require('moment');
const { GlobalPaths } = require('globalobjects');

const MessageWatcher = require(GlobalPaths.MessageWatcher);
const Log = require(GlobalPaths.Logger);
const Format = require(GlobalPaths.DiscordFormatter);
const { masterId } = require(GlobalPaths.DiscordConfig);
const DefaultGroups = require(GlobalPaths.DefaultGroups);

class CommandsWatcher extends MessageWatcher {
    constructor() {
        super(async (taylorbot, message) => {
            const { author } = message;
            if (author.bot)
                return;

            const { channel } = message;
            if (channel.type === 'text') {
                const { guild, content } = message;
                const { prefix } = taylorbot.guildSettings.get(guild.id);

                let text = content.trim();
                if (text.startsWith(prefix)) {
                    text = text.substring(prefix.length);
                    const args = text.split(' ');
                    const commandName = args.shift().toLowerCase();
                    const command = taylorbot.commandSettings.getCommand(commandName);

                    if (!command)
                        return;

                    Log.verbose(`${Format.user(author)} attempting to use '${commandName}' with args '${args.join(';')}' in ${Format.guildChannel(channel)} on ${Format.guild(guild)}.`);

                    if (!command.enabled) {
                        Log.verbose(`Command '${commandName}' can't be used because it is disabled.`);
                        return;
                    }

                    if (command.disabledIn[guild.id]) {
                        Log.verbose(`Command '${commandName}' can't be used in ${Format.guild(guild)} because it is disabled.`);
                        return;
                    }

                    if (!hasAccess()) {

                    }

                    hasAccess = (member, minimumGroupLevel, guildRoleSettings, groupSettings) => {
                        let accessLevel = member.id === masterId ? DefaultGroups.Master : DefaultGroups.Everyone;
                        if (accessLevel >= minimumGroupLevel)
                            return true;

                        const guildRoles = guildRoleSettings.get(guild.id);
                        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => g);
                        if (ownedGroups.length > 0) {
                            // TODO: foreach until you find one?
                            accessLevel = ownedGroups.reduce((a, b) =>
                                Math.max(groupSettings.get(a), groupSettings.get(b))
                            );

                            if (accessLevel >= minimumGroupLevel)
                                return true;
                        }

                        return false;
                    };

                    // TODO: Command Groups

                    const commandTime = new Date().getTime();
                    const { lastCommand, lastAnswered, ignoreUntil } = taylorbot.userSettings.get(author.id);

                    if (commandTime < ignoreUntil) {
                        Log.verbose(`Command '${commandName}' can't be used by ${Format.user(author)} because they are ignored until ${moment(ignoreUntil, 'x').format('MMM Do YY, H:mm:ss Z')}.`);
                        return;
                    }

                    if (lastAnswered < lastCommand) {
                        Log.verbose(`Command '${commandName}' can't be used by ${Format.user(author)} because they have not been answered. LastAnswered:${lastAnswered}, LastCommand:${lastCommand}.`);
                        return;
                    }

                    taylorbot.userSettings.updateLastCommand(author, commandTime);

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
                        Log.error(`Command '${commandName}' Error: ${e}`);
                    }
                    finally {
                        const answeredTime = new Date().getTime();
                        taylorbot.userSettings.updateLastAnswered(author, answeredTime);
                    }
                }
            }
        });
    }
}

module.exports = new CommandsWatcher();