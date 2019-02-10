'use strict';

const NoisyInhibitor = require('../NoisyInhibitor.js');
const Log = require('../../tools/Logger.js');
const Format = require('../../modules/DiscordFormatter.js');
const CommandMessageContext = require('../../commands/CommandMessageContext.js');
const UserGroups = require('../../client/UserGroups.js');
const { MASTER_ID } = require('../../config/config.json');

class GroupAccessInhibitor extends NoisyInhibitor {
    getBlockedMessage(messageContext, command) {
        const { message, client } = messageContext;
        const { registry } = client.master;
        const { minimumGroup } = command.command;
        if (GroupAccessInhibitor.groupHasAccess(minimumGroup.accessLevel, message, registry)) {
            return null;
        }

        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(message.author)} because they don't have the minimum group '${minimumGroup.name}'.`);

        let blockMessage = `You can't use \`${command.name}\` because it requires you to be part of the '${minimumGroup.name}' group.`;

        if (!minimumGroup.isSpecial) {
            blockMessage += `\nTo assign a role to a group, use \`${new CommandMessageContext(messageContext, registry.commands.getCommand('setrolegroup')).usage()}\`.`;
        }

        return blockMessage;
    }

    static groupHasAccess(minimumGroupLevel, message, registry) {
        const { accessLevel } = message.author.id === MASTER_ID ? UserGroups.Master : UserGroups.Everyone;

        if (accessLevel >= minimumGroupLevel)
            return true;

        const { member } = message;

        if (member) {
            if (member.guild.ownerID === member.id && UserGroups.GuildOwners.accessLevel >= minimumGroupLevel)
                return true;

            if (GroupAccessInhibitor.roleGroupHasAccess(minimumGroupLevel, member, registry.guilds, registry.groups))
                return true;
        }

        return false;
    }

    static roleGroupHasAccess(minimumGroupLevel, member, guilds, groups) {
        const guildRoles = guilds.get(member.guild.id).roleGroups;
        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => !!g);

        for (const group of ownedGroups) {
            const { accessLevel } = groups.get(group);
            if (accessLevel >= minimumGroupLevel)
                return true;
        }

        return false;
    }
}

module.exports = GroupAccessInhibitor;