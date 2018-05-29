'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const { masterId } = require(Paths.TaylorBotConfig);
const UserGroups = require(Paths.UserGroups);

class GroupAccessInhibitor extends Inhibitor {
    shouldBeBlocked({ message, client }, command) {
        if (GroupAccessInhibitor.groupHasAccess(command.command.minimumGroup.accessLevel, message, client.master.registry)) {
            return false;
        }

        Log.verbose(`Command '${command.name}' can't be used by ${Format.user(message.author)} because they don't have the minimum group '${command.command.minimumGroup.name}'.`);
        return true;
    }

    static groupHasAccess(minimumGroupLevel, message, registry) {
        const { accessLevel } = message.author.id === masterId ? UserGroups.Master : UserGroups.Everyone;

        if (accessLevel >= minimumGroupLevel)
            return true;

        const { member } = message;

        if (member) {
            if (GroupAccessInhibitor.roleGroupHasAccess(minimumGroupLevel, member, registry.guilds, registry.groups))
                return true;
        }

        return false;
    }

    static roleGroupHasAccess(minimumGroupLevel, member, guilds, groups) {
        const guildRoles = guilds.get(member.guild.id).roleGroups;
        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => g);

        for (const group of ownedGroups) {
            const { accessLevel } = groups.get(group);
            if (accessLevel >= minimumGroupLevel)
                return true;
        }

        return false;
    }
}

module.exports = GroupAccessInhibitor;